using CGZBot3.Entities.Message;
using CGZBot3.UserCommands;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections;
using System.Globalization;
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
			{ "^(\\d{2}:\\d{2}:\\d{2}\\b)", UserCommandInfo.Argument.Type.TimeSpan },
			{ "^(<@!?(\\d+)>)\\b", UserCommandInfo.Argument.Type.Member },
			{ "^(<@&(\\d+)>)\\b", UserCommandInfo.Argument.Type.Role },
			{ "^((<@&(\\d+)>)|(<@!?(\\d+)>))", UserCommandInfo.Argument.Type.Mentionable },
			{ "^(\\d+)\\b", UserCommandInfo.Argument.Type.Integer },
			{ "^((\\d+\\.\\d+)|(\\d+\\b))", UserCommandInfo.Argument.Type.Double },
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
			if (args.Guild is null) return Task.CompletedTask;

			var server = client.Servers.Single(s => s.Id == args.Guild.Id);
			var content = args.Message.Content;

			var cmds = repository.GetCommandsFor(server);
			var regex = $"^[{new string(clientOptions.Prefixes.Select(s => "\\" + s).SelectMany(s => s).ToArray())}]({string.Join('|', cmds.Select(s => $"({s.Name})"))}){"{1}"}\\b";
			var result = Regex.Matches(content, regex);

			if (!result.Any()) return Task.CompletedTask;
			var command = cmds.Single(s => s.Name == result.Single().Groups[1].Value);

			var arguments = new List<(UserCommandInfo.Argument, List<object>)>();
			var preArgs = new List<(UserCommandInfo.Argument.Type, string)>();

			//Unless no arguemnts command
			if (content.Length != command.Name.Length + 1)
			{
				var cnt = new StringBuilder(content[(command.Name.Length + 2)..]); //space include


				while (cnt.Length != 0)
				{
					try { preArgs.Add(parse(cnt)); }
					catch (Exception) { return Task.CompletedTask; }

					static (UserCommandInfo.Argument.Type, string) parse(StringBuilder cnt)
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


			var context = new UserCommandPreContext(server.GetMember(args.Author.Id), server.GetChannel(args.Channel.Id).AsText(), command,
				arguments.ToDictionary(s => s.Item1, s => (IReadOnlyList<object>)s.Item2));

			CommandWritten?.Invoke(context, (result) => CallBack(context, result, args.Message));

			return Task.CompletedTask;
		}

		private static async void CallBack(UserCommandPreContext context, UserCommandResult result, DiscordMessage cmdMsg)
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
				case UserCommandInfo.Argument.Type.Double: return double.Parse(rawStr, new CultureInfo("en-US"));
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
