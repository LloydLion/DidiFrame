using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ContextValidation.Arguments;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.UserCommands.Repository;
using DidiFrame.Utils.Collections;
using DidiFrame.Utils.ExtendableModels;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// User command pipeline dispatcher that using discord's application commands
	/// </summary>
	public class ApplicationCommandDispatcher : IUserCommandPipelineDispatcher<UserCommandPreContext>
	{
		private DispatcherSyncCallback<UserCommandPreContext>? callback;
		private readonly Dictionary<string, ApplicationCommandPair> convertedCommands;
		private readonly Client client;
		private readonly BehaviorModel behaviorModel;
		private readonly IStringLocalizer<ApplicationCommandDispatcher> localizer;
		private readonly IValidator<UserCommandResult> resultValidator;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.ApplicationCommandDispatcher
		/// </summary>
		/// <param name="dsharp">DSharp client (only DidiFrame.Clients.DSharp.Client) to attach application commands</param>
		/// <param name="commands">Repository to register application commands</param>
		/// <param name="localizer">Localizer to send error messages and write cmds' descriptions</param>
		/// <param name="converter">Converter to provide subconverters for autocomplite</param>
		/// <param name="services">Services for values providers</param>
		public ApplicationCommandDispatcher(BehaviorModel? behaviorModel, IClient dsharp, IUserCommandsRepository commands, 
			IStringLocalizer<ApplicationCommandDispatcher> localizer, IUserCommandContextConverter converter,
			IValidator<UserCommandResult> resultValidator, IServiceProvider services)
		{
			client = (Client)dsharp;
			client.BaseClient.InteractionCreated += BaseClient_InteractionCreated;

			behaviorModel = this.behaviorModel = behaviorModel ?? new BehaviorModel();
			behaviorModel.Init(client, commands, localizer, converter, services);

			this.localizer = localizer;
			this.resultValidator = resultValidator;
			convertedCommands = new();
			foreach (var cmd in commands.GetGlobalCommands())
			{
				var converted = behaviorModel.ConvertCommand(cmd);
				convertedCommands.Add(converted.DSharpCommand.Name, converted);
			}

			client.BaseClient.BulkOverwriteGlobalApplicationCommandsAsync(convertedCommands.Select(s => s.Value.DSharpCommand)).Wait();
		}


		private async Task BaseClient_InteractionCreated(DiscordClient sender, InteractionCreateEventArgs e)
		{
			if (e.Interaction.Type == InteractionType.ApplicationCommand)
			{
				var server = (Server)client.Servers.Single(s => s.Id == e.Interaction.GuildId);
				var result = await behaviorModel.ProcessInteraction(convertedCommands, e.Interaction, server);
				if (result is null) return;
				else callback?.Invoke(this, result, new(result.Invoker, result.Channel), new StateObject(e.Interaction));
			}
			else if (e.Interaction.Type == InteractionType.AutoComplete)
			{
				var server = (Server)client.Servers.Single(s => s.Id == e.Interaction.GuildId);
				await behaviorModel.ProcessAutoComplite(convertedCommands, e.Interaction, server);
			}
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
			resultValidator.ValidateAndThrow(result);

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


		private record StateObject(DiscordInteraction Interaction)
		{
			public bool Responded { get; set; }
		}

		public readonly struct ApplicationCommandPair
		{
			public ApplicationCommandPair(UserCommandInfo didiFrameCommand, DiscordApplicationCommand dSharpCommand, IReadOnlyDictionary<string, ApplicationArgumnetInfo> applicationArgumnets)
			{
				DidiFrameCommand = didiFrameCommand;
				DSharpCommand = dSharpCommand;
				ApplicationArguments = applicationArgumnets;
			}


			public UserCommandInfo DidiFrameCommand { get; }

			public DiscordApplicationCommand DSharpCommand { get; }

			public IReadOnlyDictionary<string, ApplicationArgumnetInfo> ApplicationArguments { get; }


			public readonly struct ApplicationArgumnetInfo
			{
				public ApplicationArgumnetInfo(int putIndex, UserCommandArgument.Type type, UserCommandArgument argument)
				{
					PutIndex = putIndex;
					Type = type;
					Argument = argument;
				}


				public int PutIndex { get; }

				public UserCommandArgument.Type Type { get; }

				public UserCommandArgument Argument { get; }
			}
		}

		/// <summary>
		/// Options for DidiFrame.Clients.DSharp.ApplicationCommandDispatcher
		/// </summary>
		public class Options
		{
			
		}

		public class BehaviorModel
		{
			private Client? dsharp;
			private IUserCommandsRepository? commands;
			private IStringLocalizer? localizer;
			private IUserCommandContextConverter? converter;
			private IServiceProvider? services;


			protected IUserCommandContextConverter Converter => converter ?? throw new NullReferenceException();

			protected IStringLocalizer Localizer => localizer ?? throw new NullReferenceException();

			protected IUserCommandsRepository Commands => commands ?? throw new NullReferenceException();

			protected Client Client => dsharp ?? throw new NullReferenceException();

			protected IServiceProvider Services => services ?? throw new NullReferenceException();


			public void Init(Client dsharp, IUserCommandsRepository commands, IStringLocalizer localizer, IUserCommandContextConverter converter, IServiceProvider services)
			{
				this.dsharp = dsharp;
				this.commands = commands;
				this.localizer = localizer;
				this.converter = converter;
				this.services = services;
			}


			public virtual async Task<UserCommandPreContext?> ProcessInteraction(IReadOnlyDictionary<string, ApplicationCommandPair> convertedCommands, DiscordInteraction interaction, Server server)
			{
				var channel = server.GetChannel(interaction.ChannelId).AsText();              //Get channel where interaction was created
				var member = server.GetMember(interaction.User.Id);                           //Get member who created interaction
				var cmd = convertedCommands[interaction.Data.Name];                           //Command that was called (pair - AppCmd|DidiFrameCmd)

				var preArgs = new List<object>();
				if (interaction.Data.Options is not null)
					foreach (var para in interaction.Data.Options) preArgs.Add(para.Value);

				var list = new Dictionary<UserCommandArgument, IReadOnlyList<object>>();
				try
				{
					foreach (var arg in cmd.DidiFrameCommand.Arguments)
					{
						if (arg.IsArray)
						{
							var ttype = arg.OriginTypes.Single();
							var preObjs = preArgs.Select(s => ConvertValueUp(server, s, ttype)).ToArray();
							preArgs.Clear();
							list.Add(arg, preObjs);
						}
						else
						{
							var types = arg.OriginTypes;
							var pre = preArgs.Take(types.Count).ToArray();
							var preObjs = pre.Select((s, i) => ConvertValueUp(server, s, types[i])).ToArray();
							preArgs.RemoveRange(0, types.Count);
							list.Add(arg, preObjs);
						}
					}
				}
				catch (ArgumentConvertationException ex)
				{
					await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent(localizer[ex.ErrorLocaleKey]));
					return null;
				}

				await interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
				return new UserCommandPreContext(member, channel, cmd.DidiFrameCommand, list);
			}

			public virtual async Task ProcessAutoComplite(IReadOnlyDictionary<string, ApplicationCommandPair> convertedCommands, DiscordInteraction interaction, Server server)
			{
				var channel = server.GetChannel(interaction.ChannelId).AsText();                      //Get channel where interaction was created
				var member = server.GetMember(interaction.User.Id);                                   //Get member who created interaction
				var cmd = convertedCommands[interaction.Data.Name];                                   //Command that was called (pair - AppCmd|DidiFrameCmd)

				var darg = interaction.Data.Options.Single(s => s.Focused);                           //Focused argument
				var argInfo = cmd.ApplicationArguments[darg.Name];                                      //Focused DidiFrame's argument
				var objIndex = argInfo.PutIndex;
				var arg = argInfo.Argument;
				var type = argInfo.Type;

				var providers = arg.AdditionalInfo.GetExtension<IReadOnlyCollection<IUserCommandArgumentValuesProvider>>();
				if (providers is not null)
				{
					var values = new HashSet<object>();
					if (providers.Any())
					{
						foreach (var item in providers.First().ProvideValues(server, Services)) values.Add(item);
						foreach (var prov in providers.Skip(1)) values.IntersectWith(prov.ProvideValues(server, Services));
					}

					object[] baseCollection;
					if (arg.OriginTypes.Count == 1 && arg.OriginTypes.Single().GetReqObjectType() == arg.TargetType)
						baseCollection = values.ToArray();
					else
					{
						var subconverter = Converter.GetSubConverter(arg.TargetType);
						baseCollection = values.Select(s => subconverter.ConvertBack(Services, s)[objIndex]).ToArray();
					}

					var autoComplite = baseCollection.Where(s => s is not string str || str.StartsWith((string)darg.Value))
						.Select(s =>
						{
							var ready = ConvertValueDown(s, arg.OriginTypes[objIndex]);
							return new DiscordAutoCompleteChoice(ready.ToString(), ready);
						})
						.Take(25)
						.ToArray();

					await interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(autoComplite));
				}
				else
				{
					await interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(Array.Empty<DiscordAutoCompleteChoice>()));
				}
			}

			protected virtual object ConvertValueUp(Server server, object raw, UserCommandArgument.Type type)
			{
				return type switch
				{
					UserCommandArgument.Type.DateTime => parseDate(raw),
					UserCommandArgument.Type.Role => server.GetRole((ulong)raw),
					UserCommandArgument.Type.Member => server.GetMember((ulong)raw),
					UserCommandArgument.Type.Integer => (int)(long)raw,
					UserCommandArgument.Type.String => (string)raw,
					UserCommandArgument.Type.Double => (double)raw,
					UserCommandArgument.Type.Mentionable => getMentionable((ulong)raw),
					UserCommandArgument.Type.TimeSpan => parseTime(raw),
					_ => throw new NotSupportedException(),
				};


				object getMentionable(ulong id)
				{
					return server.GetMembers().Any(s => s.Id == id) ? server.GetMember(id) : server.GetRole(id);
				}

				object parseTime(object raw)
				{
					if (TimeSpan.TryParse((string)raw, out var res)) return res;
					else throw new ArgumentConvertationException("InvalidTime");
				}

				object parseDate(object raw)
				{
					if (DateTime.TryParseExact((string)raw, "d.MM HH:mm", null, DateTimeStyles.None, out var res)) return res;
					else throw new ArgumentConvertationException("InvalidDate");
				}
			}

			protected virtual object ConvertValueDown(object didiFramePrimitive, UserCommandArgument.Type type)
			{
				return type switch
				{
					UserCommandArgument.Type.Integer => didiFramePrimitive,
					UserCommandArgument.Type.Double => didiFramePrimitive,
					UserCommandArgument.Type.String => didiFramePrimitive,
					UserCommandArgument.Type.Member => ((Member)didiFramePrimitive).Id,
					UserCommandArgument.Type.Role => ((Role)didiFramePrimitive).Id,
					UserCommandArgument.Type.Mentionable => didiFramePrimitive is Member member ? member.Id : ((Role)didiFramePrimitive).Id,
					UserCommandArgument.Type.TimeSpan => ((TimeSpan)didiFramePrimitive).ToString(),
					UserCommandArgument.Type.DateTime => ((DateTime)didiFramePrimitive).ToString("d.MM HH:mm"),
					_ => throw new NotSupportedException(),
				};
			}

			protected virtual ApplicationCommandOptionType ConvertTypeDown(UserCommandArgument.Type type)
			{
				return type switch
				{
					UserCommandArgument.Type.Integer => ApplicationCommandOptionType.Integer,
					UserCommandArgument.Type.Double => ApplicationCommandOptionType.Number,
					UserCommandArgument.Type.String => ApplicationCommandOptionType.String,
					UserCommandArgument.Type.Member => ApplicationCommandOptionType.User,
					UserCommandArgument.Type.Role => ApplicationCommandOptionType.Role,
					UserCommandArgument.Type.Mentionable => ApplicationCommandOptionType.Mentionable,
					UserCommandArgument.Type.TimeSpan => ApplicationCommandOptionType.String,
					UserCommandArgument.Type.DateTime => ApplicationCommandOptionType.String,
					_ => throw new NotSupportedException(),
				};
			}

			public virtual ApplicationCommandPair ConvertCommand(UserCommandInfo info)
			{
				var name = ConvertCommandName(info);

				var argsInfo = new Dictionary<string, ApplicationCommandPair.ApplicationArgumnetInfo>();

				var args = info.Arguments.SelectMany(s => ConvertCommandArgument(s, argsInfo)).ToArray();

				var cmdDescLocs = GetCommandDescription(info, out var mainDesc);
				var cmd = new DiscordApplicationCommand(name, mainDesc, args, description_localizations: cmdDescLocs);
				return new(info, cmd, argsInfo);
			}

			protected virtual string ConvertCommandName(UserCommandInfo command) => command.Name.Replace(" ", "_");

			protected virtual string ConvertCommandArgumentName(UserCommandArgument argument)
			{
				var splits = splitCamelCaseString(argument.Name);
				return string.Join('-', splits);


				static string[] splitCamelCaseString(string str)
				{
					var ca = str.ToCharArray();
					var res = new List<string>();
					int last = 0;

					for (int i = 0; i < ca.Length; i++)
					{
						if (char.IsUpper(ca[i]))
						{
							res.Add(str[last..i].ToLower());
							last = i;
						}
					}

					res.Add(str[last..].ToLower());

					return res.ToArray();
				}
			}

			protected virtual IEnumerable<DiscordApplicationCommandOption> ConvertCommandArgument(UserCommandArgument argument, IDictionary<string, ApplicationCommandPair.ApplicationArgumnetInfo> argsInfo)
			{
				if (argument.IsArray) return ConvertArrayCommandArgument(argument, argsInfo);
				else if (argument.OriginTypes.Count == 1) return ConvertSimpleCommandArgument(argument, argsInfo).StoreSingle();
				else return ConvertComplexCommandArgument(argument, argsInfo);
			}

			protected virtual IReadOnlyDictionary<string, string> GetCommandDescription(UserCommandInfo command, out string mainDescription)
			{
				mainDescription = Localizer["CommandDescription", command.Name];
				var r = DiscordStatic.SupportedCultures.ToDictionary(s => new CultureInfo(s));
				return Localizer.GetStringForAllLocales(r.Keys, "CommandDescription", command.Name).ToDictionary(s => r[s.Key], s => s.Value);
			}

			protected virtual IReadOnlyDictionary<string, string> GetCommandArgumentDescription(ApplicationCommandPair.ApplicationArgumnetInfo argument, out string mainDescription)
			{
				if (argument.Argument.IsArray)
				{
					mainDescription = Localizer["ArrayElementArgumentDescription", argument.Argument.Name, argument.PutIndex];
					return getDCLocs(Localizer, "ArrayElementArgumentDescription", argument.Argument.Name, argument.PutIndex);
				}
				else
				{
					if (argument.Argument.OriginTypes.Count == 1)
					{
						mainDescription = Localizer["SimpleArgumentDescription", argument.Argument.Name];
						return getDCLocs(Localizer, "SimpleArgumentDescription", argument.Argument.Name);
					}
					else
					{
						mainDescription = Localizer["ComplexArgumentDescription", argument.Argument.Name, argument.PutIndex];
						return getDCLocs(Localizer, "ComplexArgumentDescription", argument.Argument.Name, argument.PutIndex);
					}
				}


				static IReadOnlyDictionary<string, string> getDCLocs(IStringLocalizer localizer, string key, params object[] args)
				{
					var r = DiscordStatic.SupportedCultures.ToDictionary(s => new CultureInfo(s));
					var ret = localizer.GetStringForAllLocales(r.Keys, key, args).ToDictionary(s => r[s.Key], s => s.Value);
					return ret;
				}
			}

			protected virtual IEnumerable<DiscordApplicationCommandOption> ConvertArrayCommandArgument(UserCommandArgument argument, IDictionary<string, ApplicationCommandPair.ApplicationArgumnetInfo> argsInfo)
			{
				var type = ConvertTypeDown(argument.OriginTypes.Single());
				var array = new DiscordApplicationCommandOption[5];
				for (int i = 0; i < array.Length; i++)
				{
					var name = ConvertCommandArgumentName(argument) + "_" + i;
					var argInfo = new ApplicationCommandPair.ApplicationArgumnetInfo(i, argument.OriginTypes.Single(), argument);
					argsInfo.Add(name, argInfo);
					var descLocs = GetCommandArgumentDescription(argInfo, out var mainDesc);
					array[i] = new(name, mainDesc, type, required: false, description_localizations: descLocs);
				}

				return array;
			}

			protected virtual IEnumerable<DiscordApplicationCommandOption> ConvertComplexCommandArgument(UserCommandArgument argument, IDictionary<string, ApplicationCommandPair.ApplicationArgumnetInfo> argsInfo)
			{
				return argument.OriginTypes.Select((a, i) =>
				{
					var argInfo = new ApplicationCommandPair.ApplicationArgumnetInfo(i, a, argument);
					var ret = ConvertPartOfComplexCommandArgument(argInfo);
					argsInfo.Add(ret.Name, argInfo);
					return ret;
				});
			}

			protected virtual DiscordApplicationCommandOption ConvertSimpleCommandArgument(UserCommandArgument argument, IDictionary<string, ApplicationCommandPair.ApplicationArgumnetInfo> argsInfo)
			{
				var type = ConvertTypeDown(argument.OriginTypes.Single());
				var autoComp = !(type == ApplicationCommandOptionType.Mentionable || type == ApplicationCommandOptionType.Role || type == ApplicationCommandOptionType.User);
				var name = ConvertCommandArgumentName(argument);
				var argInfo = new ApplicationCommandPair.ApplicationArgumnetInfo(0, argument.OriginTypes.Single(), argument);
				argsInfo.Add(name, argInfo);
				var descLocs = GetCommandArgumentDescription(argInfo, out var mainDesc);
				return new DiscordApplicationCommandOption(name, mainDesc, type, required: true, autocomplete: autoComp, description_localizations: descLocs);
			}

			protected virtual DiscordApplicationCommandOption ConvertPartOfComplexCommandArgument(ApplicationCommandPair.ApplicationArgumnetInfo argument)
			{
				var type = ConvertTypeDown(argument.Type);
				var name = ConvertCommandArgumentName(argument.Argument) + "_" + argument.PutIndex;
				var autoComp = !(type == ApplicationCommandOptionType.Mentionable || type == ApplicationCommandOptionType.Role || type == ApplicationCommandOptionType.User);
				var descLocs = GetCommandArgumentDescription(argument, out var mainDesc);
				return new DiscordApplicationCommandOption(name, mainDesc, type, required: true, autocomplete: autoComp, description_localizations: descLocs);
			}
		}

		public class ArgumentConvertationException : Exception
		{
			public ArgumentConvertationException(string errorLocaleKey) : base(null)
			{
				ErrorLocaleKey = errorLocaleKey;
			}


			public string ErrorLocaleKey { get; }
		}
	}
}
