using DidiFrame.Interfaces;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.Repository;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DidiFrame.Clients.DSharp.Clients.DSharp
{
	public class ApplicationCommandDispatcher : IUserCommandPipelineDispatcher<UserCommandPreContext>
	{
		private DispatcherSyncCallback<UserCommandPreContext>? callback;
		private readonly Dictionary<UserCommandInfo, DiscordApplicationCommand> convertedCommands;
		private readonly Client client;


		public ApplicationCommandDispatcher(IClient dsharp, IUserCommandsRepository commands)
		{
			client = (Client)dsharp;
			client.BaseClient.InteractionCreated += BaseClient_InteractionCreated;

			var tmp = new Dictionary<UserCommandInfo, DiscordApplicationCommand>();
			foreach (var cmd in commands.GetGlobalCommands())
			{
				var converted = ConvertCommand(cmd);
				tmp.Add(cmd, converted);
			}

			convertedCommands = client.BaseClient.BulkOverwriteGlobalApplicationCommandsAsync(tmp.Values).Result
				.Join(tmp.Keys, s => s.Name, s => s.Name, (a, b) => (a, b)).ToDictionary(s => s.b, s => s.a);
		}


		private Task BaseClient_InteractionCreated(DiscordClient sender, InteractionCreateEventArgs e)
		{
			if (e.Interaction.Type == InteractionType.ApplicationCommand)
			{
				var server = (Server)client.Servers.Single(s => s.Id == e.Interaction.GuildId);
				var channel = server.GetChannel(e.Interaction.ChannelId).AsText();
				var member = server.GetMember(e.Interaction.User.Id);
				var cmdId = e.Interaction.Data.Id;
				var cmd = convertedCommands.Single(s => s.Value.Id == cmdId);

				var preArgs = new List<object>();
				foreach (var para in e.Interaction.Data.Options) preArgs.Add(para.Value);

				var list = new Dictionary<UserCommandArgument, IReadOnlyList<object>>();
				foreach (var arg in cmd.Key.Arguments)
				{
					if (arg.IsArray)
					{
						var ttype = arg.OriginTypes.Single();
						var preObjs = ReConvert(server, preArgs, new object[preArgs.Count].Select(s => ttype).ToArray());
						preArgs.Clear();
						list.Add(arg, preObjs);
					}
					else
					{
						var types = arg.OriginTypes;
						var preObjs = ReConvert(server, preArgs.Take(types.Count).ToArray(), types);
						preArgs.RemoveRange(0, types.Count);
						list.Add(arg, preObjs);
					}
				}

				e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral()).Wait();
				callback?.Invoke(this, new(member, channel, cmd.Key, list), new(member, channel), new StateObject(e.Interaction));
			}

			return Task.CompletedTask;
		}

		public void FinalizePipeline(object stateObject)
		{
			var state = (StateObject)stateObject;
			if (state.Responded == false)
			{
				var builder = new DiscordWebhookBuilder();
				builder.WithContent("Command complite");
				state.Interaction.EditOriginalResponseAsync(builder).Wait();
			}
		}

		public void Respond(object stateObject, UserCommandResult result)
		{
			var state = (StateObject)stateObject;
			if (result.RespondMessage is not null)
			{
				var builder = new DiscordWebhookBuilder();
				var msg = MessageConverter.ConvertUp(result.RespondMessage);
				builder.AddComponents(msg.Components);
				builder.AddFiles(msg.Files.ToDictionary(s => s.FileName, s => s.Stream));
				builder.AddEmbeds(msg.Embeds);
				builder.WithContent(msg.Content);
				builder.WithTTS(msg.IsTTS);
				state.Interaction.EditOriginalResponseAsync(builder).Wait();
				state.Responded = true;
			}
		}

		public void SetSyncCallback(DispatcherSyncCallback<UserCommandPreContext> callback)
		{
			this.callback = callback;
		}

		private static DiscordApplicationCommand ConvertCommand(UserCommandInfo info)
		{
			var name = info.Name.Replace(" ", "_");
			var args = info.Arguments.Select(s =>
			{
				var types = s.OriginTypes;
				if (s.IsArray)
				{
					var type = castType(s.OriginTypes.Single());
					var array = new DiscordApplicationCommandOption[5];
					for (int i = 0; i < array.Length; i++)
					{
						var name = s.Name + "_" + i;
						array[i] = new(name, $"Element of {s.Name} array", type, false);
					}

					return array;
				}
				else return types.Select((a, i) =>
				{
					var type = castType(a);
					var name = s.OriginTypes.Count == 1 ? s.Name : s.Name + "_" + i;
					return new DiscordApplicationCommandOption(name.ToLower(), "Part/full of " + s.Name + " parameter", type, true);
				}).ToArray();
			}).SelectMany(s => s).ToArray();

			return new(name, $"Invokes {info.Name} command", args);

			static ApplicationCommandOptionType castType(UserCommandArgument.Type input) => input switch
			{
				UserCommandArgument.Type.Member => ApplicationCommandOptionType.User,
				UserCommandArgument.Type.DateTime => ApplicationCommandOptionType.String,
				UserCommandArgument.Type.TimeSpan => ApplicationCommandOptionType.String,
				UserCommandArgument.Type.Role => ApplicationCommandOptionType.Role,
				UserCommandArgument.Type.Double => ApplicationCommandOptionType.Number,
				UserCommandArgument.Type.String => ApplicationCommandOptionType.String,
				UserCommandArgument.Type.Mentionable => ApplicationCommandOptionType.Mentionable,
				UserCommandArgument.Type.Integer => ApplicationCommandOptionType.Integer,
				_ => throw new NotSupportedException(),
			};
		}

		private static IReadOnlyList<object> ReConvert(Server server, IReadOnlyList<object> raw, IReadOnlyList<UserCommandArgument.Type> types)
		{
			var objs = new List<object>();

			for (int i = 0; i < raw.Count; i++)
			{
				var type = types[i];
				var rawArg = raw[i];

                object result = type switch
				{
					UserCommandArgument.Type.DateTime => DateTime.Parse((string)rawArg),
					UserCommandArgument.Type.Role => server.GetRole((ulong)rawArg),
					UserCommandArgument.Type.Member => server.GetMember((ulong)rawArg),
					UserCommandArgument.Type.Integer => (int)(long)rawArg,
					UserCommandArgument.Type.String => (string)rawArg,
					UserCommandArgument.Type.Double => (double)rawArg,
					UserCommandArgument.Type.Mentionable => throw new NotImplementedException(),
					UserCommandArgument.Type.TimeSpan => TimeSpan.Parse((string)rawArg),
					_ => throw new NotSupportedException(),
				};

				objs.Add(result);
			}

			return objs;
		}


		private record StateObject(DiscordInteraction Interaction)
		{
			public bool Responded { get; set; }
		}
	}
}
