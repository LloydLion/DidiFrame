using CGZBot3.UserCommands;
using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Collections;
using System.Text.RegularExpressions;

namespace CGZBot3.DSharpAdapter
{
	internal class CommandsDispatcher : ICommandsDispatcher
	{
		private readonly Client client;
		private readonly IUserCommandsRepository repository;
		private readonly Client.Options clientOptions;


		public CommandsDispatcher(Client client, IUserCommandsRepository repository, Client.Options clientOptions)
		{
			client.BaseClient.MessageCreated += MessageCreatedHandler;
			this.client = client;
			this.repository = repository;
			this.clientOptions = clientOptions;
		}


		public event CommandWrittenHandler? CommandWritten;


		public async Task MessageCreatedHandler(DiscordClient sender, MessageCreateEventArgs args)
		{
			var content = args.Message.Content.Split(' ');

			var prefix = clientOptions.Prefixes.Split(' ').FirstOrDefault(s => content[0].StartsWith(s));

			if (prefix is not null)
			{
				content[0] = content[0][prefix.Length..];

				var server = client.Servers.Single(s => s.Id == args.Guild.Id);

				var cmds = repository.GetCommandsFor(server);
				var cmd = cmds.SingleOrDefault(s => s.Name == content[0]);
				if (cmd is null) return;

				var parsed = new object[cmd.Arguments.Count];

				for (int i = 0; i < cmd.Arguments.Count; i++)
				{
					var arg = cmd.Arguments[i];
					try
					{
						if (arg.IsArray == false)
						{
							var rawStr = content[i + 1];

							parsed[i] = await ConvertArgumentAsync(rawStr, arg.ArgumentType, server);
						}
						else
						{
							//Array must be last
							var countOfRecived = content.Length - 1;
							var countOfTranslated = (cmd.Arguments.Count - 1);

							var countOfArray = countOfRecived - countOfTranslated;

							var result = (IList)(Activator.CreateInstance(arg.ArgumentType.GetReqObjectType().MakeArrayType(), countOfRecived)
								?? throw new ImpossibleVariantException());

							for (int j = 0; j < countOfArray; j++)
							{
								result[j] = await ConvertArgumentAsync(content[countOfTranslated + j + 1], arg.ArgumentType, server);
							}

							parsed[i] = result;
						}
					}
					catch (Exception) { return; }
				}

				var context = new UserCommandContext(await server.GetMemberAsync(args.Author.Id), (ITextChannel)await server.GetChannelAsync(args.Channel.Id), cmd,
					cmd.Arguments.Select((s, i) => (s, i)).Join(parsed.Select((s, i) => (s, i)), s => s.i, s => s.i, (a, b) => (a.s, b.s)).ToDictionary(s => s.Item1, s => s.Item2));

				CommandWritten?.Invoke(context, (result) => CallBack(context, result));
			}
		}

		private void CallBack(UserCommandContext context, UserCommandResult result)
		{
			if (result.RespondMessage is not null)
				context.Channel.SendMessageAsync(result.RespondMessage);
			else
			{
				if (result.Code != UserCommandCode.Sucssesful)
				{
					context.Channel.SendMessageAsync(new MessageSendModel("Error, command finished with code: " + result.Code));
				}
			}
		}

		private async Task<object> ConvertArgumentAsync(string rawStr, UserCommandInfo.Argument.Type ttype, IServer server)
		{
			switch (ttype)
			{
				case UserCommandInfo.Argument.Type.Integer: return int.Parse(rawStr);
				case UserCommandInfo.Argument.Type.Double: return double.Parse(rawStr);
				case UserCommandInfo.Argument.Type.String: return rawStr;
				case UserCommandInfo.Argument.Type.Member: return await server.GetMemberAsync(ulong.Parse(Regex.Match(rawStr, @"^<@!?(\d+)>$").Groups[0].Value)); //NOTE mention struct: <@USERID> or <@!USERID>
				case UserCommandInfo.Argument.Type.Role: return await server.GetRoleAsync(ulong.Parse(rawStr[2..^1])); //NOTE mention struct: <@&ROLEID>
				case UserCommandInfo.Argument.Type.Mentionable:
					if(Regex.IsMatch(rawStr, @"^<@!?(\d+)>$")) return await ConvertArgumentAsync(rawStr, UserCommandInfo.Argument.Type.Member, server);
					else return await ConvertArgumentAsync(rawStr, UserCommandInfo.Argument.Type.Role, server);
				case UserCommandInfo.Argument.Type.TimeSpan: return TimeSpan.Parse(rawStr);
				default: throw new Exception();
			}
		}
	}
}
