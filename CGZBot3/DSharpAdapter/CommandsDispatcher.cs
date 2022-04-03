using CGZBot3.Entities.Message;
using CGZBot3.UserCommands;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace CGZBot3.DSharpAdapter
{
	internal class CommandsDispatcher : ICommandsDispatcher
	{
		private readonly Client client;
		private readonly IUserCommandsRepository repository;
		private readonly Client.Options clientOptions;
		private readonly static Dictionary<string, UserCommandInfo.Argument.Type> regexes = new()
		{
			{ "^(\\d{2}.\\d{2}\\|\\d{2}:\\d{2}:\\d{2}\\b)", UserCommandInfo.Argument.Type.DateTime },
			{ "^\\d{2}:\\d{2}:\\d{2}\\b", UserCommandInfo.Argument.Type.TimeSpan },
			{ "^<@!?(\\d+)>\\b", UserCommandInfo.Argument.Type.Member },
			{ "^<@&(\\d+)>", UserCommandInfo.Argument.Type.Role },
			{ "^(<@&(\\d+)>)|(<@!?(\\d+)>)", UserCommandInfo.Argument.Type.Mentionable },
			{ "^\\d+\\b", UserCommandInfo.Argument.Type.Integer },
			{ "^(\\d+\\.\\d+)|(\\d+\\b)", UserCommandInfo.Argument.Type.Double },
			{ "^((\"[^\"]*\")|([^\\s]+))", UserCommandInfo.Argument.Type.String },
		};


		public CommandsDispatcher(Client client, IUserCommandsRepository repository, Client.Options clientOptions)
		{
			client.BaseClient.MessageCreated += MessageCreatedHandler;
			this.client = client;
			this.repository = repository;
			this.clientOptions = clientOptions;
		}


		public event CommandWrittenHandler? CommandWritten;


		public Task MessageCreatedHandler(DiscordClient sender, MessageCreateEventArgs args)
		{
			var content = args.Message.Content;
			var server = client.Servers.Single(s => s.Id == args.Guild.Id);

			var cmds = repository.GetCommandsFor(server);
			var regex = $"^[{new string(clientOptions.Prefixes.Select(s => "\\" + s).SelectMany(s => s).ToArray())}]({string.Join('|', cmds.Select(s => $"({s.Name})"))}){"{1}"}";
			var result = Regex.Matches(content, regex);

			if (!result.Any()) return Task.CompletedTask;
			var command = cmds.Single(s => s.Name == result.Single().Groups[1].Value);

			var cnt = new StringBuilder(content[(command.Name.Length + 2)..]); //space include

			var arguments = new List<object>();

			while (cnt.Length != 0)	
			{
				var isLast = command.Arguments.Count - 1 == arguments.Count;
				var lastArg = command.Arguments[command.Arguments.Count - 1];

				if (isLast && lastArg.IsArray)
				{
					var subArray = (IList)(Activator.CreateInstance(typeof(List<>).MakeGenericType(lastArg.ArgumentType.GetReqObjectType())) ?? throw new ImpossibleVariantException());

					while (cnt.Length != 0)
					{
						var val = parse();
						if (val.HasValue == false) return Task.CompletedTask;
						subArray.Add(ConvertArgument(val.Value.Item2, val.Value.Item1, server));
					}

					arguments.Add(subArray.GetType()?.GetMethod("ToArray")?.Invoke(subArray, Array.Empty<object>()) ?? throw new ImpossibleVariantException());
				}
				else
				{
					var val = parse();
					if (val.HasValue == false) return Task.CompletedTask;
					arguments.Add(ConvertArgument(val.Value.Item2, val.Value.Item1, server));
				}


				(UserCommandInfo.Argument.Type, string)? parse()
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

					return null;
				}
			}

			if (arguments.Count != command.Arguments.Count) return Task.CompletedTask;
			

			var context = new UserCommandContext(server.GetMember(args.Author.Id), server.GetChannel(args.Channel.Id).AsText(), command,
				command.Arguments.Select((s, i) => (s, i)).Join(arguments.Select((s, i) => (s, i)), s => s.i, s => s.i, (a, b) => (a.s, b.s)).ToDictionary(s => s.Item1, s => s.Item2));

			CommandWritten?.Invoke(context, (result) => CallBack(context, result, args.Message));

			return Task.CompletedTask;
		}

		private static async void CallBack(UserCommandContext context, UserCommandResult result, DiscordMessage cmdMsg)
		{
			IMessage? msg = null;

			if (result.RespondMessage is not null)
				msg = await context.Channel.SendMessageAsync(result.RespondMessage);
			else
			{
				if (result.Code != UserCommandCode.Sucssesful)
				{
					msg = await context.Channel.SendMessageAsync(new MessageSendModel("Error, command finished with code: " + result.Code));
				}
			}

			await Task.Delay(10000);
			if (msg is not null) try { await msg.DeleteAsync(); } catch(Exception) { }
			try { await cmdMsg.DeleteAsync(); } catch (Exception) { }
		}

		private object ConvertArgument(string rawStr, UserCommandInfo.Argument.Type ttype, IServer server)
		{
			switch (ttype)
			{
				case UserCommandInfo.Argument.Type.Integer: return int.Parse(rawStr);
				case UserCommandInfo.Argument.Type.Double: return double.Parse(rawStr);
				case UserCommandInfo.Argument.Type.String: return rawStr.StartsWith('\"') && rawStr.EndsWith('\"') ? rawStr[1..^1] : rawStr;
				case UserCommandInfo.Argument.Type.Member:
					var match = Regex.Match(rawStr, @"^<@!?(\d+)>$").Groups[1].Value;
					var id = ulong.Parse(match);
					return server.GetMember(id); //NOTE mention struct: <@USERID> or <@!USERID>
				case UserCommandInfo.Argument.Type.Role: return server.GetRole(ulong.Parse(rawStr[2..^1])); //NOTE mention struct: <@&ROLEID>
				case UserCommandInfo.Argument.Type.Mentionable:
					if(Regex.IsMatch(rawStr, @"^<@!?(\d+)>$")) return ConvertArgument(rawStr, UserCommandInfo.Argument.Type.Member, server);
					else return ConvertArgument(rawStr, UserCommandInfo.Argument.Type.Role, server);
				case UserCommandInfo.Argument.Type.TimeSpan: return TimeSpan.Parse(rawStr);
				case UserCommandInfo.Argument.Type.DateTime:
					var split = rawStr.Split('|');
					return new DateTime(DateTime.Parse(split[0] + '.' + DateTime.Now.Year + ' ' + split[1]).Ticks, DateTimeKind.Local);
				default: throw new Exception();
			}
		}
	}
}
