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
		private readonly IStringLocalizer<ApplicationCommandDispatcher> localizer;
		private readonly IUserCommandContextConverter converter;
		private readonly IServiceProvider services;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.ApplicationCommandDispatcher
		/// </summary>
		/// <param name="dsharp">DSharp client (only DidiFrame.Clients.DSharp.Client) to attach application commands</param>
		/// <param name="commands">Repository to register application commands</param>
		/// <param name="localizer">Localizer to send error messages and write cmds' descriptions</param>
		/// <param name="converter">Converter to provide subconverters for autocomplite</param>
		/// <param name="services">Services for values providers</param>
		public ApplicationCommandDispatcher(IClient dsharp, IUserCommandsRepository commands, IStringLocalizer<ApplicationCommandDispatcher> localizer, IUserCommandContextConverter converter, IServiceProvider services)
		{
			client = (Client)dsharp;
			client.BaseClient.InteractionCreated += BaseClient_InteractionCreated;
			this.localizer = localizer;
			this.converter = converter;
			this.services = services;

			convertedCommands = new();
			foreach (var cmd in commands.GetGlobalCommands())
			{
				var converted = ConvertCommand(cmd);
				convertedCommands.Add(converted.DSharpCommand.Name, converted);
			}

			client.BaseClient.BulkOverwriteGlobalApplicationCommandsAsync(convertedCommands.Select(s => s.Value.DSharpCommand)).Wait();
		}


		private Task BaseClient_InteractionCreated(DiscordClient sender, InteractionCreateEventArgs e)
		{
			if (e.Interaction.Type == InteractionType.ApplicationCommand)
			{
				var server = (Server)client.Servers.Single(s => s.Id == e.Interaction.GuildId);	//Get server where interaction was created
				var channel = server.GetChannel(e.Interaction.ChannelId).AsText();				//Get channel where interaction was created
				var member = server.GetMember(e.Interaction.User.Id);							//Get member who created interaction
				var cmd = convertedCommands[e.Interaction.Data.Name];							//Command that was called (pair - AppCmd|DidiFrameCmd)

				var preArgs = new List<object>();
				if (e.Interaction.Data.Options is not null)
					foreach (var para in e.Interaction.Data.Options) preArgs.Add(para.Value);

				var list = new Dictionary<UserCommandArgument, IReadOnlyList<object>>();
				foreach (var arg in cmd.DidiFrameCommand.Arguments)
				{
					if (arg.IsArray)
					{
						var ttype = arg.OriginTypes.Single();
						string? error = null;
						var preObjs = preArgs.Select(s => ConvertValueUp(server, s, ttype, out error)).ToArray();
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
						var preObjs = pre.Select((s, i) => ConvertValueUp(server, s, types[i], out error)).ToArray();
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
				callback?.Invoke(this, new(member, channel, cmd.DidiFrameCommand, list), new(member, channel), new StateObject(e.Interaction));
			}
			else if (e.Interaction.Type == InteractionType.AutoComplete)
			{
				var server = (Server)client.Servers.Single(s => s.Id == e.Interaction.GuildId);			//Get server where interaction was created
				var channel = server.GetChannel(e.Interaction.ChannelId).AsText();						//Get channel where interaction was created
				var member = server.GetMember(e.Interaction.User.Id);									//Get member who created interaction
				var cmd = convertedCommands[e.Interaction.Data.Name];									//Command that was called (pair - AppCmd|DidiFrameCmd)

				var darg = e.Interaction.Data.Options.Single(s => s.Focused);                           //Focused argument
				var argInfo = cmd.ApplicationArguments[darg.Name];										//Focused DidiFrame's argument
				var objIndex = argInfo.PutIndex;
				var arg = argInfo.Argument;
				var type = argInfo.Type;

				var providers = arg.AdditionalInfo.GetExtension<IReadOnlyCollection<IUserCommandArgumentValuesProvider>>();
				if (providers is not null)
				{
					var values = new HashSet<object>();
					if (providers.Any())
					{
						foreach (var item in providers.First().ProvideValues(server, services)) values.Add(item);
						foreach (var prov in providers.Skip(1)) values.IntersectWith(prov.ProvideValues(server, services));
					}

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
							var ready = ConvertValueDown(s, arg.OriginTypes[objIndex]);
							return new DiscordAutoCompleteChoice(ready.ToString(), ready);
						})
						.Take(25)
						.ToArray();

					e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(autoComplite)).Wait();
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

		private ApplicationCommandPair ConvertCommand(UserCommandInfo info)
		{
			var name = info.Name.Replace(" ", "_");

			var argsInfo = new Dictionary<string, ApplicationCommandPair.ApplicationArgumnetInfo>();

			var args = info.Arguments.Select(s =>
			{
				if (s.IsArray)
				{
					var type = ConvertTypeDown(s.OriginTypes.Single());
					var array = new DiscordApplicationCommandOption[5];
					for (int i = 0; i < array.Length; i++)
					{
						var name = convertArgName(s.Name) + "_" + i;
						var descLocs = getDCLocs(localizer, DiscordStatic.SupportedCultures, "ArrayElementArgumentDescription", s.Name, i);
						array[i] = new(name, localizer["ArrayElementArgumentDescription", s.Name, i], type, required: false, description_localizations: descLocs);
						argsInfo.Add(name, new(i, s.OriginTypes.Single(), s));
					}

					return array;
				}
				else if (s.OriginTypes.Count == 1)
				{
					var type = ConvertTypeDown(s.OriginTypes.Single());
					var autoComp = !(type == ApplicationCommandOptionType.Mentionable || type == ApplicationCommandOptionType.Role || type == ApplicationCommandOptionType.User);
					var name = convertArgName(s.Name);
					var descLocs = getDCLocs(localizer, DiscordStatic.SupportedCultures, "SimpleArgumentDescription", s.Name);
					argsInfo.Add(name, new(0, s.OriginTypes.Single(), s));
					return new DiscordApplicationCommandOption(name, localizer["SimpleArgumentDescription", s.Name], type, required: true, autocomplete: autoComp, description_localizations: descLocs).StoreSingle();
				}
				else return s.OriginTypes.Select((a, i) =>
				{
					var type = ConvertTypeDown(a);
					var name = convertArgName(s.Name) + "_" + i;
					var autoComp = !(type == ApplicationCommandOptionType.Mentionable || type == ApplicationCommandOptionType.Role || type == ApplicationCommandOptionType.User);
					var descLocs = getDCLocs(localizer, DiscordStatic.SupportedCultures, "ComplexArgumentDescription", i, s.Name);
					argsInfo.Add(name, new(i, a, s));
					return new DiscordApplicationCommandOption(name, localizer["ComplexArgumentDescription", i, s.Name], type, required: true, autocomplete: autoComp, description_localizations: descLocs);
				}).ToArray();
			}).SelectMany(s => s).ToArray();

			var cmdDescLocs = getDCLocs(localizer, DiscordStatic.SupportedCultures, "CommandDescription", info.Name);
			var cmd = new DiscordApplicationCommand(name, localizer["CommandDescription", info.Name], args, description_localizations: cmdDescLocs);
			return new(info, cmd, argsInfo);


			static IReadOnlyDictionary<string, string> getDCLocs(IStringLocalizer localizer, IReadOnlyCollection<string> cultures, string key, params object[] args)
			{
				var r = cultures.ToDictionary(s => new CultureInfo(s));
				var ret = localizer.GetStringForAllLocales(r.Keys, key, args).ToDictionary(s => r[s.Key], s => s.Value);
				return ret;
			}
			
			static string convertArgName(string orinalName)
			{
				var splits = SplitCamelCaseString(orinalName);
				return string.Join('-', splits);
			}
		}

		private static object? ConvertValueUp(Server server, object raw, UserCommandArgument.Type type, out string? error)
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
				if (DateTime.TryParseExact((string)raw, "d.MM HH:mm", null, DateTimeStyles.None, out var res)) return res;
				else error = "InvalidDate";
				return null;
			}
		}

		private static object ConvertValueDown(object didiFramePrimitive, UserCommandArgument.Type type)
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

		private static ApplicationCommandOptionType ConvertTypeDown(UserCommandArgument.Type type)
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
		
		private static string[] SplitCamelCaseString(string str)
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


		private record StateObject(DiscordInteraction Interaction)
		{
			public bool Responded { get; set; }
		}

		private readonly struct ApplicationCommandPair
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
	}
}
