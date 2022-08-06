using System.Globalization;
using System.Text.RegularExpressions;

namespace DidiFrame.UserCommands.Pipeline.ClassicPipeline
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
		/// <param name="behaviorModel">Optional behavior model that can override object behavior</param>
		public TextCommandParser(IOptions<Options> options, IUserCommandsRepository repository, BehaviorModel? behaviorModel = null)
		{
			this.repository = repository;
			behaviorModel = this.behaviorModel = behaviorModel ?? new BehaviorModel();
			behaviorModel.Init(options.Value);
		}


		/// <inheritdoc/>
		public override UserCommandMiddlewareExcutionResult<UserCommandPreContext> Process(string content, UserCommandPipelineContext pipelineContext)
		{
			var server = pipelineContext.SendData.Channel.Server;
			var cmds = repository.GetFullCommandList(server);
			var command = behaviorModel.ParseCommandName(content, cmds);

			if (command is null)
			{
				throw new UserCommandPipelineDropException($"Command not found for string: {content}");
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
						selects.Add(behaviorModel.SelectArgument(span, out span));
					}
				}

				var selectsArray = selects.ToArray();
				var selectsSpan = (ReadOnlySpan<ArgumentSelect>)selectsArray.AsSpan();

				foreach (var argument in command.Arguments)
				{

					var result = behaviorModel.ProcessArgument(selectsSpan, argument, server, out selectsSpan);
					arguments.Add(argument, result);
				}

				return new UserCommandPreContext(pipelineContext.SendData, command, arguments);
			}
			else
			{
				if (command.Arguments.Any())
				{
					throw new UserCommandPipelineDropException("Original content doesn't contain arguments, but command contains them");
				}
				else
				{
					return new UserCommandPreContext(pipelineContext.SendData, command, new Dictionary<UserCommandArgument, IReadOnlyList<object>>());
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

		/// <summary>
		/// Segment of input string with determined type
		/// </summary>
		public struct ArgumentSelect
		{
			/// <summary>
			/// Creates new instance of DidiFrame.UserCommands.Pipeline.Utils.TextCommandParser.ArgumentSelect
			/// </summary>
			/// <param name="selectedString">Input string framgment</param>
			/// <param name="type">Type of argument</param>
			public ArgumentSelect(string selectedString, UserCommandArgument.Type type)
			{
				SelectedString = selectedString;
				Type = type;
			}


			/// <summary>
			/// Input string framgment
			/// </summary>
			public string SelectedString { get; }

			/// <summary>
			/// Type of argument
			/// </summary>
			public UserCommandArgument.Type Type { get; }
		}

		/// <summary>
		/// Behavoir model for idiFrame.UserCommands.Pipeline.Utils.TextCommandParser
		/// </summary>
		public class BehaviorModel
		{
			private Options? options;


			/// <summary>
			/// Options for idiFrame.UserCommands.Pipeline.Utils.TextCommandParser
			/// </summary>
			public Options Options => options ?? throw new NullReferenceException();


			internal void Init(Options options)
			{
				this.options = options;
			}

			/// <summary>
			/// Parses name of command
			/// </summary>
			/// <param name="rawInput">Raw input string</param>
			/// <param name="commands">Collecition of commands in server</param>
			/// <returns>User command info</returns>
			public virtual UserCommandInfo? ParseCommandName(string rawInput, IUserCommandsCollection commands)
			{
				var regex = $"^[{new string(Options.Prefixes.SelectMany(s => "\\" + s).ToArray())}]({string.Join('|', commands.Select(s => $"({s.Name})"))}){"{1}"}\\b";
				var result = Regex.Matches(rawInput, regex);
				if (!result.Any()) return null;
				else return commands.GetCommad(result.Single().Groups[1].Value);
			}

			/// <summary>
			/// Converts raw argument string to raw user command argument value
			/// </summary>
			/// <param name="rawStr">Raw argument string</param>
			/// <param name="ttype">Target type of convertation</param>
			/// <param name="server">Server where need to convert string</param>
			/// <returns>Convertation result</returns>
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

			/// <summary>
			/// Selects argument from input string and slices given string to continue parsing
			/// </summary>
			/// <param name="rawString">String value input</param>
			/// <param name="newString">Sliced string output</param>
			/// <returns>Argument select object</returns>
			/// <exception cref="InvalidOperationException">If input string is invalid</exception>
			public virtual ArgumentSelect SelectArgument(ReadOnlySpan<char> rawString, out ReadOnlySpan<char> newString)
			{
				foreach (var key in regexes.Keys)
				{
					var macthes = Regex.Matches(new string(rawString), key);
					if (macthes.Any() == false) continue;
					else
					{
						var match = macthes.Single();
						var rawStr = match.Groups[1].Value;
						newString = rawString[rawStr.Length..];
						return new(rawStr, regexes[key]);
					}
				}

				throw new InvalidOperationException("Enable to select some command's argument");
			}

			/// <summary>
			/// Processes single user command argument and slices selects collection with required raw arguments
			/// </summary>
			/// <param name="selects">All availiable raw arguments and output for sliced raw arguments</param>
			/// <param name="argument">Argument to process</param>
			/// <param name="server">Server where command was writen</param>
			/// <param name="unusedSelects">Used selects span output</param>
			/// <returns>List of ready objects or null if cannot process argument</returns>
			public virtual IReadOnlyList<object> ProcessArgument(ReadOnlySpan<ArgumentSelect> selects, UserCommandArgument argument, IServer server, out ReadOnlySpan<ArgumentSelect> unusedSelects)
			{
				var result = new List<object>();

				if (argument.IsArray)
				{
					foreach (var item in selects)
						result.Add(ConvertArgumentValue(item.SelectedString, item.Type, server));
					unusedSelects = ReadOnlySpan<ArgumentSelect>.Empty;
				}
				else
				{
					var pa = selects[..argument.OriginTypes.Count];

					if (pa.Length != argument.OriginTypes.Count)
						throw new ArgumentException("Enable to select argument from given selects", nameof(selects));
					for (int i = 0; i < pa.Length; i++)
						if (argument.OriginTypes[i] != pa[i].Type)
							throw new ArgumentException("Enable to select argument from given selects", nameof(selects));

					foreach (var item in pa)
						result.Add(ConvertArgumentValue(item.SelectedString, item.Type, server));
					unusedSelects = selects[argument.OriginTypes.Count..];
				}

				return result;
			}
		}
	}
}
