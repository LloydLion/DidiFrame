using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DidiFrame.UserCommands.Pipeline.Utils
{
	/// <summary>
	/// Parser for text commands
	/// </summary>
	public class TextCommandParser : AbstractUserCommandPipelineMiddleware<string, UserCommandPreContext>
	{
		private readonly BehaviorModel behaviorModel;
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


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.Utils.TextCommandParser
		/// </summary>
		/// <param name="options">Option for parser (DidiFrame.UserCommands.Pipeline.Utils.TextCommandParser.Options)</param>
		/// <param name="repository">Command repository to detect commands</param>
		public TextCommandParser(BehaviorModel? behaviorModel, IOptions<Options> options, IUserCommandsRepository repository)
		{
			this.repository = repository;
			behaviorModel = this.behaviorModel = behaviorModel ?? new BehaviorModel();
			behaviorModel.Init(options.Value);
		}


		/// <inheritdoc/>
		public override UserCommandPreContext? Process(string content, UserCommandPipelineContext pipelineContext)
		{
			var server = pipelineContext.SendData.Channel.Server;
			var cmds = repository.GetCommandsFor(server);
			var command = behaviorModel.ParseCommandName(content, cmds);

			if (command is null)
			{
				pipelineContext.DropPipeline();
				return null;
			}

			var arguments = new Dictionary<UserCommandArgument, IReadOnlyList<object>>();
			var selects = new List<ArgumentSelect>();

			//Unless no arguemnts command
			if (content.Length != command.Name.Length + 1)
			{
				var span = content.AsSpan(command.Name.Length + 1); //prefix include

				while (span.Length != 0)
				{
					span = span[1..];

					var matches = Regex.Matches(new string(span), "^\\s+");
					if (matches.Count == 1)
					{
						var slice = matches.Single().ValueSpan.Length;
						span = span[slice..];
					}
					else
					{
						try
						{
							selects.Add(behaviorModel.SelectArgument(ref span));
						}
						catch (Exception)
						{
							pipelineContext.DropPipeline();
							return null;
						}
					}
				}

				var selectsArray = selects.ToArray();
				var selectsSpan = (ReadOnlySpan<ArgumentSelect>)selectsArray.AsSpan();

				foreach (var argument in command.Arguments)
				{
					var result = behaviorModel.ProcessArgument(ref selectsSpan, argument, server);
					if (result is null)
					{
						pipelineContext.DropPipeline();
						return null;
					}

					arguments.Add(argument, result);
				}

				return new(pipelineContext.SendData.Invoker, pipelineContext.SendData.Channel, command, arguments);
			}
			else
			{
				if (command.Arguments.Any())
				{
					pipelineContext.DropPipeline();
					return null;
				}
				else
				{
					return new(pipelineContext.SendData.Invoker, pipelineContext.SendData.Channel, command, new Dictionary<UserCommandArgument, IReadOnlyList<object>>());
				}
			}
		}


		/// <summary>
		/// Options for DidiFrame.UserCommands.Pipeline.Utils.TextCommandParser
		/// </summary>
		public class Options
		{
			/// <summary>
			/// Char array of prefixes by that command can start
			/// </summary>
			public string Prefixes { get; set; } = "";
		}

		public struct ArgumentSelect
		{
			public ArgumentSelect(string selectedString, UserCommandArgument.Type type)
			{
				SelectedString = selectedString;
				Type = type;
			}


			public string SelectedString { get; }

			public UserCommandArgument.Type Type { get; }
		}

		public class BehaviorModel
		{
			private Options? options;


			public Options Options => options ?? throw new NullReferenceException();


			public void Init(Options options)
			{
				this.options = options;
			}

			public virtual UserCommandInfo? ParseCommandName(string rawInput, IUserCommandsCollection commands)
			{
				var regex = $"^[{new string(Options.Prefixes.SelectMany(s => "\\" + s).ToArray())}]({string.Join('|', commands.Select(s => $"({s.Name})"))}){"{1}"}\\b";
				var result = Regex.Matches(rawInput, regex);
				if (!result.Any()) return null;
				else return commands.GetCommad(result.Single().Groups[1].Value);
			}

			protected virtual object ConvertArgumentValue(string rawStr, UserCommandArgument.Type ttype, IServer server)
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
						if (Regex.IsMatch(rawStr, @"^<@!?(\d+)>$")) return ConvertArgumentValue(rawStr, UserCommandArgument.Type.Member, server);
						else return ConvertArgumentValue(rawStr, UserCommandArgument.Type.Role, server);
					case UserCommandArgument.Type.TimeSpan: return TimeSpan.Parse(rawStr);
					case UserCommandArgument.Type.DateTime:
						var split = rawStr.Split('|');
						return new DateTime(DateTime.Parse(split[0] + '.' + DateTime.Now.Year + ' ' + split[1]).Ticks, DateTimeKind.Local);
					default:
						throw new ImpossibleVariantException();
				}
			}

			public virtual ArgumentSelect SelectArgument(ref ReadOnlySpan<char> rawString)
			{
				foreach (var key in regexes.Keys)
				{
					var macthes = Regex.Matches(new string(rawString), key);
					if (macthes.Any() == false) continue;
					else
					{
						var match = macthes.Single();
						var rawStr = match.Groups[1].Value;
						rawString = rawString[..rawStr.Length];
						return new(rawStr, regexes[key]);
					}
				}

				throw new InvalidOperationException("Enable to select some command's argument");
			}

			public virtual IReadOnlyList<object>? ProcessArgument(ref ReadOnlySpan<ArgumentSelect> selects, UserCommandArgument argument, IServer server)
			{
				var result = new List<object>();

				if (argument.IsArray)
				{
					foreach (var item in selects)
						result.Add(ConvertArgumentValue(item.SelectedString, item.Type, server));
					selects = ReadOnlySpan<ArgumentSelect>.Empty;
				}
				else
				{
					var pa = selects[..argument.OriginTypes.Count];

					if (pa.Length != argument.OriginTypes.Count)
					{
						return null;
					}
					for (int i = 0; i < pa.Length; i++)
						if (argument.OriginTypes[i] != pa[i].Type)
						{
							return null;
						}

					foreach (var item in pa)
						result.Add(ConvertArgumentValue(item.SelectedString, item.Type, server));
					selects = selects[argument.OriginTypes.Count..];
				}

				return result;
			}
		}
	}
}
