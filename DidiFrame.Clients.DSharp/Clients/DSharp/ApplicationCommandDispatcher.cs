using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ContextValidation.Arguments;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.UserCommands.Repository;
using DidiFrame.Utils.ExtendableModels;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace DidiFrame.Clients.DSharp.Clients.DSharp
{
	public class ApplicationCommandDispatcher : IUserCommandPipelineDispatcher<UserCommandPreContext>
	{
		private DispatcherSyncCallback<UserCommandPreContext>? callback;
		private readonly Dictionary<UserCommandInfo, DiscordApplicationCommand> convertedCommands;
		private readonly Client client;
		private readonly IStringLocalizer<ApplicationCommandDispatcher> localizer;
		private readonly IUserCommandContextConverter converter;
		private readonly IServiceProvider services;


		public ApplicationCommandDispatcher(IClient dsharp, IUserCommandsRepository commands, IStringLocalizer<ApplicationCommandDispatcher> localizer, IUserCommandContextConverter converter, IServiceProvider services)
		{
			client = (Client)dsharp;
			client.BaseClient.InteractionCreated += BaseClient_InteractionCreated;
			this.localizer = localizer;
			this.converter = converter;
			this.services = services;
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
				var server = (Server)client.Servers.Single(s => s.Id == e.Interaction.GuildId);         //Get server where interaction was created
				var channel = server.GetChannel(e.Interaction.ChannelId).AsText();                      //Get channel where interaction was created
				var member = server.GetMember(e.Interaction.User.Id);                                   //Get member who created interaction
				var cmdId = e.Interaction.Data.Id;                                                      //Id of called command
				var cmd = convertedCommands.Single(s => s.Value.Id == cmdId);                           //Command that was called (pair - AppCmd|DidiFrameCmd)

				var preArgs = new List<object>();
				if (e.Interaction.Data.Options is not null)
					foreach (var para in e.Interaction.Data.Options) preArgs.Add(para.Value);

				var list = new Dictionary<UserCommandArgument, IReadOnlyList<object>>();
				foreach (var arg in cmd.Key.Arguments)
				{
					if (arg.IsArray)
					{
						var ttype = arg.OriginTypes.Single();
						string? error = null;
						var preObjs = preArgs.Select(s => ReConvert(server, s, ttype, out error)).ToArray();
						preArgs.Clear();

						if (error is not null)
						{
							e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent(localizer[error]));
							return Task.CompletedTask;
						}
						else
#pragma warning disable CS8620
							list.Add(arg, preObjs);
#pragma warning restore CS8620
					}
					else
					{
						var types = arg.OriginTypes;
						var pre = preArgs.Take(types.Count).ToArray();
						string? error = null;
						var preObjs = types.Select((s, i) => ReConvert(server, s, types[i], out error)).ToArray();
						preArgs.RemoveRange(0, types.Count);

						if (error is not null)
						{
							e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent(localizer[error]));
							return Task.CompletedTask;
						}
						else
#pragma warning disable CS8620
							list.Add(arg, preObjs);
#pragma warning restore CS8620
					}
				}

				e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral()).Wait();
				callback?.Invoke(this, new(member, channel, cmd.Key, list), new(member, channel), new StateObject(e.Interaction));
			}
			else if (e.Interaction.Type == InteractionType.AutoComplete)
			{
				var server = (Server)client.Servers.Single(s => s.Id == e.Interaction.GuildId);			//Get server where interaction was created
				var channel = server.GetChannel(e.Interaction.ChannelId).AsText();						//Get channel where interaction was created
				var member = server.GetMember(e.Interaction.User.Id);                                   //Get member who created interaction
				var cmdId = e.Interaction.Data.Id;														//Id of called command
				var cmd = convertedCommands.Single(s => s.Value.Id == cmdId);							//Command that was called (pair - AppCmd|DidiFrameCmd)

				var darg = e.Interaction.Data.Options.Single(s => s.Focused);                           //Focused argument
				var processedName = darg.Name.Contains('_') ? darg.Name.Split('_')[0] : darg.Name;		//Processed name of arguments that will be used if argumnet is complex
				var arg = cmd.Key.Arguments.Single(s => s.Name == darg.Name || s.Name == processedName);//Focused DidiFrame's argument
				var objIndex = processedName == darg.Name ? 0 : int.Parse(darg.Name.Split('_')[1]);

				var providers = arg.AdditionalInfo.GetExtension<IReadOnlyCollection<IUserCommandArgumentValuesProvider>>();
				if (providers is not null)
				{
					var values = new HashSet<object>();
					foreach (var prov in providers)
						foreach (var el in prov.ProvideValues(server, services))
							values.Add(el);

					object[] baseCollection;
					if (arg.OriginTypes.Count == 1 && arg.OriginTypes.Single().GetReqObjectType() == arg.TargetType)
						baseCollection = values.ToArray();
					else
					{
						var subconverter = converter.GetSubConverter(arg.TargetType);
						baseCollection = values.Select(s => subconverter.ConvertBack(services, s)[objIndex]).ToArray();
					}

					var autoComplite = baseCollection.Where(s => s is not string str || str.StartsWith((string)darg.Value))
						.Select(s =>
						{
							var ready = tConvert(arg.OriginTypes[objIndex], s);
							return new DiscordAutoCompleteChoice(ready.ToString(), ready);
						})
						.Take(25)
						.ToArray();

					e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(autoComplite)).Wait();


					static object tConvert(UserCommandArgument.Type type, object originalObj)
					{
						return type switch
						{
							UserCommandArgument.Type.Integer => originalObj,
							UserCommandArgument.Type.Double => originalObj,
							UserCommandArgument.Type.String => originalObj,
							UserCommandArgument.Type.Member => ((IMember)originalObj).Id,
							UserCommandArgument.Type.Role => ((IRole)originalObj).Id,
							UserCommandArgument.Type.Mentionable => originalObj is IMember member ? member.Id : ((IRole)originalObj).Id,
							UserCommandArgument.Type.TimeSpan => originalObj.ToString() ?? throw new ImpossibleVariantException(),
							UserCommandArgument.Type.DateTime => originalObj.ToString() ?? throw new ImpossibleVariantException(),
							_ => throw new NotSupportedException()
						};
					}
				}
				else
				{
					e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(Array.Empty<DiscordAutoCompleteChoice>())).Wait();
				}
			}

			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public void FinalizePipeline(object stateObject)
		{
			var state = (StateObject)stateObject;
			if (state.Responded == false)
			{
				var builder = new DiscordWebhookBuilder();
				builder.WithContent(localizer["CommandComplited"]);
				state.Interaction.EditOriginalResponseAsync(builder).Wait();
			}
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public void SetSyncCallback(DispatcherSyncCallback<UserCommandPreContext> callback)
		{
			this.callback = callback;
		}

		private DiscordApplicationCommand ConvertCommand(UserCommandInfo info)
		{
			var name = info.Name.Replace(" ", "_");
			var args = info.Arguments.Select(s =>
			{
				if (s.IsArray)
				{
					var type = castType(s.OriginTypes.Single());
					var array = new DiscordApplicationCommandOption[5];
					for (int i = 0; i < array.Length; i++)
					{
						var name = s.Name + "_" + i;
						var descLocs = getDCLocs(localizer, DiscordStatic.SupportedCultures, "ArrayElementArgumentDescription", s.Name, i);
						array[i] = new(name, localizer["ArrayElementArgumentDescription", s.Name, i], type, required: false, description_localizations: descLocs);
					}

					return array;
				}
				else if (s.OriginTypes.Count == 1)
				{
					var type = castType(s.OriginTypes.Single());
					var autoComp = !(type == ApplicationCommandOptionType.Mentionable || type == ApplicationCommandOptionType.Role || type == ApplicationCommandOptionType.User);
					var descLocs = getDCLocs(localizer, DiscordStatic.SupportedCultures, "SimpleArgumentDescription", s.Name);
					return new[] { new DiscordApplicationCommandOption(s.Name.ToLower(), localizer["SimpleArgumentDescription", s.Name], type, required: true, autocomplete: autoComp, description_localizations: descLocs) };
				}
				else return s.OriginTypes.Select((a, i) =>
				{
					var type = castType(a);
					var name = s.Name + "_" + i;
					var autoComp = !(type == ApplicationCommandOptionType.Mentionable || type == ApplicationCommandOptionType.Role || type == ApplicationCommandOptionType.User);
					var descLocs = getDCLocs(localizer, DiscordStatic.SupportedCultures, "ComplexArgumentDescription", i, s.Name);
					return new DiscordApplicationCommandOption(name.ToLower(), localizer["ComplexArgumentDescription", i, s.Name], type, required: true, autocomplete: autoComp, description_localizations: descLocs);
				}).ToArray();
			}).SelectMany(s => s).ToArray();

			var cmdDescLocs = getDCLocs(localizer, DiscordStatic.SupportedCultures, "CommandDescription", info.Name);
			return new(name, localizer["CommandDescription", info.Name], args, description_localizations: cmdDescLocs);

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

			static IReadOnlyDictionary<string, string> getDCLocs(IStringLocalizer localizer, IReadOnlyCollection<string> cultures, string key, params object[] args)
			{
				var r = cultures.ToDictionary(s => new CultureInfo(s));
				var ret = localizer.GetStringForAllLocales(r.Keys, key, args).ToDictionary(s => r[s.Key], s => s.Value);
				return ret;
			}
		}

		private static object? ReConvert(Server server, object raw, UserCommandArgument.Type type, out string? error)
		{
			error = null;

			return type switch
			{
				UserCommandArgument.Type.DateTime => parseDate(raw, out error),
				UserCommandArgument.Type.Role => server.GetRole((ulong)raw),
				UserCommandArgument.Type.Member => server.GetMember((ulong)raw),
				UserCommandArgument.Type.Integer => (int)(long)raw,
				UserCommandArgument.Type.String => (string)raw,
				UserCommandArgument.Type.Double => (double)raw,
				UserCommandArgument.Type.Mentionable => getMentionable((ulong)raw),
				UserCommandArgument.Type.TimeSpan => parseTime(raw, out error),
				_ => throw new NotSupportedException(),
			};


			object getMentionable(ulong id)
			{
				return server.GetMembers().Any(s => s.Id == id) ? server.GetMember(id) : server.GetRole(id);
			}

			object? parseTime(object raw, out string? error)
			{
				error = null;
				if (TimeSpan.TryParse((string)raw, out var res)) return res;
				else error = "InvalidTime";
				return null;
			}

			object? parseDate(object raw, out string? error)
			{
				error = null;
				if (DateTime.TryParse((string)raw, out var res)) return res;
				else error = "InvalidDate";
				return null;
			}
		}


		private record StateObject(DiscordInteraction Interaction)
		{
			public bool Responded { get; set; }
		}
	}
}
