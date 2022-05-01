using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DidiFrame.UserCommands.Pipeline.Utils
{
	public class TextCommandParser : AbstractUserCommandPipelineMiddleware<string, UserCommandPreContext>
	{
		private readonly Options options;
		private readonly IUserCommandsRepository repository;
		private readonly static Dictionary<string, UserCommandArgument.Type> regexes = new()
		{
			{ "^(\\d{2}.\\d{2}\\|\\d{2}:\\d{2}:\\d{2}\\b)", UserCommandArgument.Type.DateTime },
			{ "^(\\d{2}:\\d{2}:\\d{2}\\b)", UserCommandArgument.Type.TimeSpan },
			{ "^(<@!?(\\d+)>)\\b", UserCommandArgument.Type.Member },
			{ "^(<@&(\\d+)>)\\b", UserCommandArgument.Type.Role },
			{ "^((<@&(\\d+)>)|(<@!?(\\d+)>))", UserCommandArgument.Type.Mentionable },
			{ "^(\\d+)\\b", UserCommandArgument.Type.Integer },
			{ "^((\\d+\\.\\d+)|(\\d+\\b))", UserCommandArgument.Type.Double },
			{ "^((\"[^\"]*\")|([^\\s]+))", UserCommandArgument.Type.String },
		};


		public TextCommandParser(IOptions<Options> options, IUserCommandsRepository repository)
		{
			this.options = options.Value;
			this.repository = repository;
		}


		public override UserCommandPreContext? Process(string content, UserCommandPipelineContext pipelineContext)
		{
			var server = pipelineContext.SendData.Channel.Server;
			var cmds = repository.GetCommandsFor(server);
			var regex = $"^[{new string(options.Prefixes.Select(s => "\\" + s).SelectMany(s => s).ToArray())}]({string.Join('|', cmds.Select(s => $"({s.Name})"))}){"{1}"}\\b";
			var result = Regex.Matches(content, regex);

			if (!result.Any())
			{
				pipelineContext.DropPipeline();
				return null;
			}

			var command = cmds.Single(s => s.Name == result.Single().Groups[1].Value);

			var arguments = new List<(UserCommandArgument, List<object>)>();
			var preArgs = new List<(UserCommandArgument.Type, string)>();

			//Unless no arguemnts command
			if (content.Length != command.Name.Length + 1)
			{
				var cnt = new StringBuilder(content[(command.Name.Length + 2)..]); //space include


				while (cnt.Length != 0)
				{
					try { preArgs.Add(parse(cnt)); }
					catch (Exception)
					{
						pipelineContext.DropPipeline();
						return null;
					}

					static (UserCommandArgument.Type, string) parse(StringBuilder cnt)
					{
						foreach (var key in regexes.Keys)
						{
							var macthes = Regex.Matches(cnt.ToString(), key);
							if (macthes.Any() == false) continue;
							else
							{
								var match = macthes.Single();
								var rawStr = match.Groups[1].Value;
								if (cnt.Length == rawStr.Length) cnt.Clear();
								else cnt.Remove(0, rawStr.Length + 1);
								return (regexes[key], rawStr);
							}
						}

						throw new InvalidOperationException("Enable to parse some command's argument");
					}
				}

				foreach (var argument in command.Arguments)
				{
					if (argument.IsArray)
						arguments.Add((argument, preArgs.Select(s => ConvertArgument(s.Item2, s.Item1, server)).ToList()));
					else
					{
						//order is important (thanks LINQ)
						var pa = preArgs.Take(argument.OriginTypes.Count);
						arguments.Add((argument, pa.Select(s => ConvertArgument(s.Item2, s.Item1, server)).ToList()));
						preArgs.RemoveRange(0, argument.OriginTypes.Count);
					}
				}
			}

			return new(pipelineContext.SendData.Invoker, pipelineContext.SendData.Channel, command, arguments.ToDictionary(s => s.Item1, s => (IReadOnlyList<object>)s.Item2));
		}

		private object ConvertArgument(string rawStr, UserCommandArgument.Type ttype, IServer server)
		{
			switch (ttype)
			{
				case UserCommandArgument.Type.Integer: return int.Parse(rawStr);
				case UserCommandArgument.Type.Double: return double.Parse(rawStr, new CultureInfo("en-US"));
				case UserCommandArgument.Type.String: return rawStr.StartsWith('\"') && rawStr.EndsWith('\"') ? rawStr[1..^1] : rawStr;
				case UserCommandArgument.Type.Member:
					var match = Regex.Match(rawStr, @"^<@!?(\d+)>$").Groups[1].Value;
					var id = ulong.Parse(match);
					return server.GetMember(id); //NOTE mention struct: <@USERID> or <@!USERID>
				case UserCommandArgument.Type.Role: return server.GetRole(ulong.Parse(rawStr[2..^1])); //NOTE mention struct: <@&ROLEID>
				case UserCommandArgument.Type.Mentionable:
					if (Regex.IsMatch(rawStr, @"^<@!?(\d+)>$")) return ConvertArgument(rawStr, UserCommandArgument.Type.Member, server);
					else return ConvertArgument(rawStr, UserCommandArgument.Type.Role, server);
				case UserCommandArgument.Type.TimeSpan: return TimeSpan.Parse(rawStr);
				case UserCommandArgument.Type.DateTime:
					var split = rawStr.Split('|');
					return new DateTime(DateTime.Parse(split[0] + '.' + DateTime.Now.Year + ' ' + split[1]).Ticks, DateTimeKind.Local);
				default: throw new Exception();
			}
		}


		public class Options
		{
			public string Prefixes { get; set; } = "";
		}
	}
}
