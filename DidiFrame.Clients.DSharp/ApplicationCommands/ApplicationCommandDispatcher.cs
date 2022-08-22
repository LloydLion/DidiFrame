using DidiFrame.Clients.DSharp.Entities;
using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Clients;
using DidiFrame.UserCommands.ContextValidation.Arguments;
using DidiFrame.UserCommands.Modals;
using DidiFrame.UserCommands.Modals.Components;
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
using DidiFrame.Clients.DSharp;

namespace DidiFrame.Clients.DSharp.ApplicationCommands
{
	/// <summary>
	/// User command pipeline dispatcher that using discord's application commands
	/// </summary>
	public sealed class ApplicationCommandDispatcher : IUserCommandPipelineDispatcher<UserCommandPreContext>, IDisposable
	{
		private DispatcherCallback<UserCommandPreContext>? callback;
		private readonly Dictionary<string, ApplicationCommandPair> convertedCommands;
		private readonly DSharpClient client;
		private readonly BehaviorModel behaviorModel;
		private readonly IStringLocalizer<ApplicationCommandDispatcher> localizer;
		private readonly IValidator<UserCommandResult> resultValidator;
		private readonly ModalHelper modalHelper;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.ApplicationCommandDispatcher
		/// </summary>
		/// <param name="dsharp">DSharp client (only DidiFrame.Clients.DSharp.Client) to attach application commands</param>
		/// <param name="commands">Repository to register application commands</param>
		/// <param name="localizer">Localizer to send error messages and write cmds' descriptions</param>
		/// <param name="converter">Converter to provide subconverters for autocomplite</param>
		/// <param name="resultValidator">Validator for DidiFrame.UserCommands.Models.UserCommandResult type</param>
		/// <param name="services">Services for values providers</param>
		/// <param name="behaviorModel">Behavoir model that can override behavior of component</param>
		public ApplicationCommandDispatcher(IClient dsharp, IUserCommandsRepository commands,
			IStringLocalizer<ApplicationCommandDispatcher> localizer, IUserCommandContextConverter converter,
			IValidator<UserCommandResult> resultValidator, IServiceProvider services, IValidator<ModalModel> modalValidator, BehaviorModel? behaviorModel = null)
		{
			client = (DSharpClient)dsharp;
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

			modalHelper = new ModalHelper(client, localizer, modalValidator);
		}


		private async Task BaseClient_InteractionCreated(DiscordClient sender, InteractionCreateEventArgs e)
		{
			if (e.Interaction.Type == InteractionType.ApplicationCommand)
			{
				var server = client.GetServer(e.Interaction.GuildId ?? throw new ImpossibleVariantException());
				var result = await behaviorModel.ProcessInteraction(convertedCommands, e.Interaction, server);
				if (result is null) return;
				else callback?.Invoke(this, result, new(result.SendData.Invoker, result.SendData.Channel), new StateObject(e.Interaction));
			}
			else if (e.Interaction.Type == InteractionType.AutoComplete)
			{
				var server = client.GetServer(e.Interaction.GuildId ?? throw new ImpossibleVariantException());
				await behaviorModel.ProcessAutoComplite(convertedCommands, e.Interaction, server);
			}
		}

		/// <inheritdoc/>
		public void FinalizePipeline(object stateObject)
		{
			var state = (StateObject)stateObject;
			if (state.Responded == false)
			{
				state.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(localizer["CommandComplited"])).Wait();
			}
		}

		/// <inheritdoc/>
		public async Task RespondAsync(object stateObject, UserCommandResult result)
		{
			resultValidator.ValidateAndThrow(result);

			var state = (StateObject)stateObject;

			switch (result.ResultType)
			{
				case UserCommandResult.Type.Message:
					var server = client.GetServer(state.Interaction.GuildId ?? throw new ImpossibleVariantException());

					var msg = MessageConverter.ConvertUp(result.GetRespondMessage());
					await state.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder(msg).AsEphemeral());

					var subscriber = result.GetInteractionDispatcherSubcriber();
					if (subscriber is not null)
					{
						var responce = await state.Interaction.GetOriginalResponseAsync();
						var dispatcher = server.CreateTemporalDispatcherForUnregistedMessage(responce);
						subscriber(dispatcher);
					}

					break;
				case UserCommandResult.Type.Modal:
					await modalHelper.CreateModalAsync(state.Interaction, result.GetModal());
					break;
				case UserCommandResult.Type.None:
					break;
				default:
					throw new NotSupportedException("Target type of user command result is not supported by dispatcher");
			}

			state.Responded = true;
		}

		/// <inheritdoc/>
		public void SetSyncCallback(DispatcherCallback<UserCommandPreContext> callback)
		{
			this.callback = callback;
		}

		public void Dispose()
		{
			modalHelper.Dispose();
		}


		private sealed record StateObject(DiscordInteraction Interaction)
		{
			public bool Responded { get; set; }
		}


		/// <summary>
		/// Represents a pair of user command object and converted discord application command with addititional raw arguments info
		/// </summary>
		public readonly struct ApplicationCommandPair
		{
			/// <summary>
			/// Creates new instance of DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair
			/// </summary>
			/// <param name="didiFrameCommand">User command object</param>
			/// <param name="dSharpCommand">Converted discord application command</param>
			/// <param name="applicationArgumnets">Dictionary raw argument name to raw argument info</param>
			public ApplicationCommandPair(UserCommandInfo didiFrameCommand, DiscordApplicationCommand dSharpCommand, IReadOnlyDictionary<string, ApplicationArgumnetInfo> applicationArgumnets)
			{
				DidiFrameCommand = didiFrameCommand;
				DSharpCommand = dSharpCommand;
				ApplicationArguments = applicationArgumnets;
			}


			/// <summary>
			/// User command object
			/// </summary>
			public UserCommandInfo DidiFrameCommand { get; }

			/// <summary>
			/// Converted discord application command
			/// </summary>
			public DiscordApplicationCommand DSharpCommand { get; }

			/// <summary>
			/// Dictionary raw argument name to raw argument info
			/// </summary>
			public IReadOnlyDictionary<string, ApplicationArgumnetInfo> ApplicationArguments { get; }


			/// <summary>
			/// Represents a raw argument info for discord application commands
			/// </summary>
			public readonly struct ApplicationArgumnetInfo
			{
				/// <summary>
				/// Creates new instance of DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair.ApplicationArgumnetInfo
				/// </summary>
				/// <param name="putIndex">Index of part of complex argument value or array put index</param>
				/// <param name="type">Type of this raw argument</param>
				/// <param name="argument">Base argument info, one argument info can be used for multiple ApplicationArgumnetInfo if it is array or complex arg</param>
				public ApplicationArgumnetInfo(int putIndex, UserCommandArgument.Type type, UserCommandArgument argument)
				{
					PutIndex = putIndex;
					Type = type;
					Argument = argument;
				}


				/// <summary>
				/// Index of part of complex argument value or array put index
				/// </summary>
				public int PutIndex { get; }

				/// <summary>
				/// Type of this raw argument
				/// </summary>
				public UserCommandArgument.Type Type { get; }

				/// <summary>
				/// Base argument info, one argument info can be used for multiple ApplicationArgumnetInfo if it is array or complex arg
				/// </summary>
				public UserCommandArgument Argument { get; }
			}
		}

		/// <summary>
		/// Options for DidiFrame.Clients.DSharp.ApplicationCommandDispatcher
		/// </summary>
		public sealed class Options
		{

		}

		/// <summary>
		/// Behavior model for DidiFrame.Clients.DSharp.ApplicationCommandDispatcher class that can override its behavior
		/// </summary>
		public class BehaviorModel
		{
			private DSharpClient? dsharp;
			private IUserCommandsRepository? commands;
			private IStringLocalizer<ApplicationCommandDispatcher>? localizer;
			private IUserCommandContextConverter? converter;
			private IServiceProvider? services;


			/// <summary>
			/// Converter to provide subconverters for autocomplite
			/// </summary>
			protected IUserCommandContextConverter Converter => converter ?? throw new InvalidOperationException("Enable to get this property in ctor");

			/// <summary>
			/// Localizer to send error messages and write cmds' descriptions
			/// </summary>
			protected IStringLocalizer<ApplicationCommandDispatcher> Localizer => localizer ?? throw new InvalidOperationException("Enable to get this property in ctor");

			/// <summary>
			/// Repository to register application commands
			/// </summary>
			protected IUserCommandsRepository Commands => commands ?? throw new InvalidOperationException("Enable to get this property in ctor");

			/// <summary>
			/// DSharp client (only DidiFrame.Clients.DSharp.Client)
			/// </summary>
			protected DSharpClient Client => dsharp ?? throw new InvalidOperationException("Enable to get this property in ctor");

			/// <summary>
			/// Services for values providers
			/// </summary>
			protected IServiceProvider Services => services ?? throw new InvalidOperationException("Enable to get this property in ctor");


			internal void Init(DSharpClient dsharp, IUserCommandsRepository commands, IStringLocalizer<ApplicationCommandDispatcher> localizer, IUserCommandContextConverter converter, IServiceProvider services)
			{
				this.dsharp = dsharp;
				this.commands = commands;
				this.localizer = localizer;
				this.converter = converter;
				this.services = services;
			}


			/// <summary>
			/// Processes discord interaction call
			/// </summary>
			/// <param name="convertedCommands">Avaliable commands</param>
			/// <param name="interaction">Interaction dsharp wrap</param>
			/// <param name="server">Server where interaction has benn called</param>
			/// <returns>Task with pre user command context that contains info about raw arguments</returns>
			public virtual async Task<UserCommandPreContext?> ProcessInteraction(IReadOnlyDictionary<string, ApplicationCommandPair> convertedCommands, DiscordInteraction interaction, ServerWrap server)
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
					await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent(Localizer[ex.ErrorLocaleKey]));
					return null;
				}

				return new UserCommandPreContext(new(member, channel), cmd.DidiFrameCommand, list);
			}

			/// <summary>
			/// Processes discord autocomplite call
			/// </summary>
			/// <param name="convertedCommands">Avaliable commands</param>
			/// <param name="interaction">Interaction dsharp wrap</param>
			/// <param name="server">Server where interaction has benn called</param>
			/// <returns>Operation wait task</returns>
			public virtual async Task ProcessAutoComplite(IReadOnlyDictionary<string, ApplicationCommandPair> convertedCommands, DiscordInteraction interaction, ServerWrap server)
			{
				var channel = server.GetChannel(interaction.ChannelId).AsText();                      //Get channel where interaction was created
				var member = server.GetMember(interaction.User.Id);                                   //Get member who created interaction
				var cmd = convertedCommands[interaction.Data.Name];                                   //Command that was called (pair - AppCmd|DidiFrameCmd)

				var darg = interaction.Data.Options.Single(s => s.Focused);                           //Focused argument
				var argInfo = cmd.ApplicationArguments[darg.Name];                                    //Focused DidiFrame's argument

				var objIndex = argInfo.PutIndex;
				var arg = argInfo.Argument;

				var sendData = new UserCommandSendData(member, channel);

				var providers = arg.AdditionalInfo.GetExtension<IReadOnlyCollection<IUserCommandArgumentValuesProvider>>();
				if (providers is not null)
				{
					var values = new HashSet<object>();
					if (providers.Any())
					{
						foreach (var item in providers.First().ProvideValues(sendData)) values.Add(item);
						foreach (var prov in providers.Skip(1)) values.IntersectWith(prov.ProvideValues(sendData));
					}

					object[] baseCollection;
					if (arg.OriginTypes.Count == 1 && arg.OriginTypes.Single().GetReqObjectType() == arg.TargetType)
						baseCollection = values.ToArray();
					else
					{
						var subconverter = Converter.GetSubConverter(arg.TargetType);
						baseCollection = values.Select(s => subconverter.ConvertBack(s).PreArguments[objIndex]).ToArray();
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

			/// <summary>
			/// Converts value from discord user command to workable object
			/// </summary>
			/// <param name="server">Server where need to convert object</param>
			/// <param name="raw">Raw value from discord</param>
			/// <param name="type">Target type of convertation</param>
			/// <returns>Workable object</returns>
			/// <exception cref="NotSupportedException">If target convertation type is not supported</exception>
			/// <exception cref="ArgumentConvertationException">If input has invalid foramt for some types</exception>
			/// <exception cref="InvalidCastException">If input has invalid type</exception>
			protected virtual object ConvertValueUp(ServerWrap server, object raw, UserCommandArgument.Type type)
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

			/// <summary>
			/// Converts workable object to value for discord user command
			/// </summary>
			/// <param name="didiFramePrimitive">Workablr object</param>
			/// <param name="type">Original type of object</param>
			/// <returns>Value for discord user command</returns>
			/// <exception cref="NotSupportedException">If original type is not supported</exception>
			/// <exception cref="InvalidCastException">If input has invalid type</exception>
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

			/// <summary>
			/// Converts didiFrame user command argument type to discord application command argument type
			/// </summary>
			/// <param name="type">Type to convert</param>
			/// <returns>Discord application command argument type</returns>
			/// <exception cref="NotSupportedException">If original type is not supported</exception>
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

			/// <summary>
			/// Converts user command object to discord application object and puts it to DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair object
			/// </summary>
			/// <param name="info">Original user command info</param>
			/// <returns>DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair pair woth commands and arguments</returns>
			public virtual ApplicationCommandPair ConvertCommand(UserCommandInfo info)
			{
				var name = ConvertCommandName(info);

				var argsInfo = new Dictionary<string, ApplicationCommandPair.ApplicationArgumnetInfo>();

				var args = info.Arguments.SelectMany(s => ConvertCommandArgument(s, argsInfo)).ToArray();

				var cmdDescLocs = GetCommandDescription(info, out var mainDesc);
				var cmd = new DiscordApplicationCommand(name, mainDesc, args, description_localizations: cmdDescLocs);
				return new(info, cmd, argsInfo);
			}

			/// <summary>
			/// Gets name for discord application command from user command info
			/// </summary>
			/// <param name="command">User command info</param>
			/// <returns>Name for discord application command</returns>
			protected virtual string ConvertCommandName(UserCommandInfo command) => command.Name.Replace(" ", "_");

			/// <summary>
			/// Gets name for discord application command argument from user command argument
			/// </summary>
			/// <param name="argument">User command argument</param>
			/// <returns>Name for discord application command argument</returns>
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

			/// <summary>
			/// Converts user command argument to enumerable of discord application command arguments and pushes
			///	DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair.ApplicationArgumnetInfo to given dictionary 
			/// </summary>
			/// <param name="argument">User command argument</param>
			/// <param name="argsInfo">Dictionary to push DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair.ApplicationArgumnetInfo instances</param>
			/// <returns>Enumerable of discord application command arguments for given user command argument</returns>
			protected virtual IEnumerable<DiscordApplicationCommandOption> ConvertCommandArgument(UserCommandArgument argument, IDictionary<string, ApplicationCommandPair.ApplicationArgumnetInfo> argsInfo)
			{
				if (argument.IsArray) return ConvertArrayCommandArgument(argument, argsInfo);
				else if (argument.OriginTypes.Count == 1) return ConvertSimpleCommandArgument(argument, argsInfo).StoreSingle();
				else return ConvertComplexCommandArgument(argument, argsInfo);
			}

			/// <summary>
			/// Provides description and its localizations for discord application command
			/// </summary>
			/// <param name="command">Target user command info</param>
			/// <param name="mainDescription">Out parameter with main description</param>
			/// <returns>Dictionary discord culture key to description localization</returns>
			protected virtual IReadOnlyDictionary<string, string> GetCommandDescription(UserCommandInfo command, out string mainDescription)
			{
				mainDescription = Localizer["CommandDescription", command.Name];
				var r = DiscordStatic.SupportedCultures.ToDictionary(s => new CultureInfo(s));
				return Localizer.GetStringForAllLocales(r.Keys, "CommandDescription", command.Name).ToDictionary(s => r[s.Key], s => s.Value);
			}

			/// <summary>
			/// Provides description and its localizations for discord application command argument
			/// </summary>
			/// <param name="argument">Target user command argument</param>
			/// <param name="mainDescription">Out parameter with main description</param>
			/// <returns>Dictionary discord culture key to description localization</returns>
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

			/// <summary>
			/// Converts array user command argument to enumerable of discord application command arguments and pushes
			///	DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair.ApplicationArgumnetInfo to given dictionary
			/// </summary>
			/// <param name="argument">User command argument of array type</param>
			/// <param name="argsInfo">Dictionary to push DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair.ApplicationArgumnetInfo instances</param>
			/// <returns>Enumerable of discord application command arguments for given user command argument</returns>
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

			/// <summary>
			/// Converts complex (multiple input values) user command argument to enumerable of discord application command arguments and pushes
			///	DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair.ApplicationArgumnetInfo to given dictionary
			/// </summary>
			/// <param name="argument">User command argument of complex (multiple input values) type</param>
			/// <param name="argsInfo">Dictionary to push DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair.ApplicationArgumnetInfo instances</param>
			/// <returns>Enumerable of discord application command arguments for given user command argument</returns>
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

			/// <summary>
			/// Converts simple (single input value) user command argument to enumerable of discord application command arguments and pushes
			///	DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair.ApplicationArgumnetInfo to given dictionary
			/// </summary>
			/// <param name="argument">User command argument of simple (single input value) type</param>
			/// <param name="argsInfo">Dictionary to push DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair.ApplicationArgumnetInfo instances</param>
			/// <returns>Discord application command argument for given user command argument</returns>
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

			/// <summary>
			/// Converts part of complex (multiple input values) user command argument to enumerable of discord application command arguments
			/// </summary>
			/// <param name="argument">DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ApplicationCommandPair.ApplicationArgumnetInfo object</param>
			/// <returns>Discord application command argument for given part of user command argument</returns>
			protected virtual DiscordApplicationCommandOption ConvertPartOfComplexCommandArgument(ApplicationCommandPair.ApplicationArgumnetInfo argument)
			{
				var type = ConvertTypeDown(argument.Type);
				var name = ConvertCommandArgumentName(argument.Argument) + "_" + argument.PutIndex;
				var autoComp = !(type == ApplicationCommandOptionType.Mentionable || type == ApplicationCommandOptionType.Role || type == ApplicationCommandOptionType.User);
				var descLocs = GetCommandArgumentDescription(argument, out var mainDesc);
				return new DiscordApplicationCommandOption(name, mainDesc, type, required: true, autocomplete: autoComp, description_localizations: descLocs);
			}
		}

		/// <summary>
		/// Represents error while argument value convertation
		/// </summary>
		public sealed class ArgumentConvertationException : Exception
		{
			/// <summary>
			/// Creates new instance of DidiFrame.Clients.DSharp.ApplicationCommandDispatcher.ArgumentConvertationException
			/// </summary>
			/// <param name="errorLocaleKey">Locale key in DidiFrame.Clients.DSharp.ApplicationCommandDispatcher</param>
			public ArgumentConvertationException(string errorLocaleKey) : base(null)
			{
				ErrorLocaleKey = errorLocaleKey;
			}


			/// <summary>
			/// Locale key in DidiFrame.Clients.DSharp.ApplicationCommandDispatcher
			/// </summary>
			public string ErrorLocaleKey { get; }
		}
	}
}
