using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.UserCommands.ContextValidation.Arguments.Providers;
using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.Utils;
using DidiFrame.Utils.ExtendableModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace DidiFrame.UserCommands.Loader.EmbededCommands.Help
{
	/// <summary>
	/// Help and cmd commands loader
	/// </summary>
	public class HelpCommandLoader : IUserCommandsLoader
	{
		private const int MaxCmdsInOnePage = 20;


		private readonly IUserCommandsRepository repository;
		private readonly IUserCommandContextConverter converter;
		private readonly BehaviorModel behaviorModel;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.EmbededCommands.Help.HelpCommandLoader
		/// </summary>
		/// <param name="localizer">Localizer for commands</param>
		/// <param name="repository">Commands repositroy to get commands</param>
		/// <param name="converter">Converter to get values provider from DidiFrame.UserCommands.Models.UserCommandInfo subconverter</param>
		public HelpCommandLoader(IStringLocalizer<HelpCommandLoader> localizer, IUserCommandsRepository repository, IUserCommandContextConverter converter, BehaviorModel? behaviorModel = null)
		{
			this.repository = repository;
			this.converter = converter;
			behaviorModel = this.behaviorModel = behaviorModel ?? new BehaviorModel();
			behaviorModel.Init(localizer);
		}


		/// <inheritdoc/>
		public void LoadTo(IUserCommandsRepository rp)
		{
			rp.AddCommand(behaviorModel.LoadCmdCommand(CmdHandler, repository, converter));
			rp.AddCommand(behaviorModel.LoadHelpCommand(HelpHandler, repository, converter));
		}


		private Task<UserCommandResult> HelpHandler(UserCommandContext ctx)
		{
			var page = ctx.Arguments.Single().Value.As<int>();

			var cmds = repository.GetFullCommandList(ctx.Channel.Server);

			var data = cmds.Skip((page - 1) * MaxCmdsInOnePage).Take(MaxCmdsInOnePage).ToArray();

			var tablet = behaviorModel.CreateHelpTablet(data);

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new() { MessageEmbeds = new[] { tablet } } });
		}

		private Task<UserCommandResult> CmdHandler(UserCommandContext ctx)
		{
			var message = behaviorModel.ExecuteCMDCommand(ctx.Arguments.Single().Value.As<UserCommandInfo>(), out var executeCode);

			return Task.FromResult(new UserCommandResult(executeCode) { RespondMessage = message });
		}


		private class Provider : IUserCommandArgumentValuesProvider
		{
			private readonly IUserCommandsRepository repository;


			public Provider(IUserCommandsRepository repository)
			{
				this.repository = repository;
			}


			public Type TargetType => typeof(int);


			public IReadOnlyCollection<object> ProvideValues(IServer server, IServiceProvider services)
			{
				var max = repository.GetFullCommandList(server).Count / MaxCmdsInOnePage; //Max page INDEX
				return new Collection(max + 1);
			}


			private class Collection : IReadOnlyCollection<object>
			{
				private readonly int max;


				public Collection(int max)
				{
					this.max = max;
				}


				public int Count => max;


				public IEnumerator<object> GetEnumerator()
				{
					for (int i = 1; i <= max; i++) yield return i;
				}

				IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			}
		}

		public class BehaviorModel
		{
			private IStringLocalizer<HelpCommandLoader>? localizer;


			protected IStringLocalizer<HelpCommandLoader> Localizer => localizer ?? throw new InvalidOperationException("Enable to get this property in ctor");


			public void Init(IStringLocalizer<HelpCommandLoader> localizer)
			{
				this.localizer = localizer;
			}


			public virtual MessageEmbed CreateHelpTablet(IReadOnlyCollection<UserCommandInfo> commands)
			{
				var embedBuilder = new MessageEmbedBuilder(Localizer["HelpTitle"], Localizer["HelpDescription"], new("#44dca5"));

				foreach (var cmd in commands)
				{
					var desc = cmd.AdditionalInfo.GetExtension<CommandDescription>();
					var cmdloc = cmd.AdditionalInfo.GetExtension<IStringLocalizer>();

					if (desc is not null && cmdloc is not null) embedBuilder.AddField(new(Localizer["HelpCommandName", cmd.Name], cmdloc[desc.ShortSpecify]));
					else embedBuilder.AddField(new(Localizer["HelpCommandName", cmd.Name], Localizer["NoDataProvided"]));
				}

				return embedBuilder.Build();
			}

			public virtual MessageSendModel ExecuteCMDCommand(UserCommandInfo command, out UserCommandCode executeCode)
			{
				var desc = command.AdditionalInfo.GetExtension<CommandDescription>();
				var cmdloc = command.AdditionalInfo.GetExtension<IStringLocalizer>();

				if (desc is null || cmdloc is null)
				{
					executeCode = UserCommandCode.InternalError;
					return new(Localizer["NoDataProvided"]);
				}

				var tablet = CreateCommandTablet(command);

				executeCode = UserCommandCode.Sucssesful;
				return new() { MessageEmbeds = new[] { tablet } };
			}


			protected virtual MessageEmbed CreateCommandTablet(UserCommandInfo command)
			{
				var desc = command.AdditionalInfo.GetRequiredExtension<CommandDescription>();
				var cmdloc = command.AdditionalInfo.GetRequiredExtension<IStringLocalizer>();

				var title = desc.DescribeGroup is null ? Localizer["CmdTitle_1", cmdloc[command.Name]] : Localizer["CmdTitle_2", cmdloc[command.Name], cmdloc[desc.DescribeGroup]];
				var embedBuilder = new MessageEmbedBuilder(title, Localizer["CmdDescription", cmdloc[desc.Description]], new("#fc44a1"));

				//Execution group
				if (desc.LaunchGroup != LaunchGroup.Everyone || desc.LaunchDescription is not null)
				{
					string executionGroupContent;
					if (desc.LaunchDescription is null) executionGroupContent = Localizer["ExecutionGroupContent_1", Localizer["LaunchGroup:" + desc.LaunchGroup.ToString()]];
					else executionGroupContent = Localizer["ExecutionGroupContent_2", Localizer["LaunchGroup:" + desc.LaunchGroup.ToString()], cmdloc[desc.LaunchDescription]];
					embedBuilder.AddField(new(Localizer["ExecutionGroupTitle"], executionGroupContent));
				}

				//Using
				var usingStr = $"{command.Name} " + string.Join(' ', command.Arguments.Select(s => s.IsArray ? $"[{s.Name} (Array of {s.OriginTypes.Single()})]..." : $"[{s.Name} ({string.Join(", ", s.OriginTypes)})]"));
				embedBuilder.AddField(new(Localizer["UsingTitle"], usingStr));

				//Args desc
				foreach (var arg in command.Arguments)
				{
					var argDesc = arg.AdditionalInfo.GetExtension<ArgumentDescription>();
					embedBuilder.AddField(new EmbedField(Localizer["ArgumentInfoTitle", arg.Name], argDesc is null ? Localizer["NoDataProvided"] : cmdloc[argDesc.ShortSpecify]));
				}

				//Remarks
				var args = command.Arguments.Where(s => s.AdditionalInfo.GetExtension<ArgumentDescription>()?.Remarks is not null).ToArray();
				var rmStrs = args.Select(s => Localizer["RemarkContent_2", s.Name, cmdloc[s.AdditionalInfo.GetRequiredExtension<ArgumentDescription>().Remarks ?? throw new ImpossibleVariantException()]]);
				var cmdRemark = desc.Remarks is null ? "" : '*' + Localizer["RemarkContent_1", cmdloc[desc.Remarks]] + '\n';

				if (string.IsNullOrWhiteSpace(cmdRemark) == false || rmStrs.Any())
				{
					var readyRemarks = cmdRemark + '*' + string.Join("\n*", rmStrs);
					embedBuilder.AddField(new(Localizer["RemarksTitle"], readyRemarks));
				}


				return embedBuilder.Build();
			}

			public virtual UserCommandInfo LoadHelpCommand(UserCommandHandler helpHandler, IUserCommandsRepository repository, IUserCommandContextConverter converter)
			{
				var dic = new Dictionary<Type, object>
				{
					{ typeof(IReadOnlyCollection<IUserCommandArgumentValuesProvider>), new IUserCommandArgumentValuesProvider[] { new Provider(repository) } },
					{ typeof(LocaleMap), new LocaleMap(new Dictionary<string, string>() { { ContextValidator.ProviderErrorCode, "NoPage" } }) },
					{ typeof(ArgumentDescription), new ArgumentDescription("TargetPage", null) }
				};

				return new UserCommandInfo("help", helpHandler, new UserCommandArgument[]
				{
					new(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "page", new SimpleModelAdditionalInfoProvider(dic))
				}, new SimpleModelAdditionalInfoProvider((Localizer, typeof(IStringLocalizer)),
					(new CommandDescription("ShowsCmdsList", "ShowsCmdsList", LaunchGroup.Everyone, null, null, "Help"), typeof(CommandDescription))));
			}

			public virtual UserCommandInfo LoadCmdCommand(UserCommandHandler cmdHandler, IUserCommandsRepository repository, IUserCommandContextConverter converter)
			{
				var dic = new Dictionary<Type, object>
				{
					{ typeof(ArgumentDescription), new ArgumentDescription("TargetCmd", null) },
				};

				var provider = converter.GetSubConverter(typeof(UserCommandInfo)).CreatePossibleValuesProvider();
				if (provider is not null)
				{
					dic.Add(typeof(IReadOnlyCollection<IUserCommandArgumentValuesProvider>), new[] { provider });
					dic.Add(typeof(LocaleMap), new LocaleMap(new Dictionary<string, string>() { { ContextValidator.ProviderErrorCode, "NoCommandFound" } }));
				}

				return new UserCommandInfo("cmd", cmdHandler, new UserCommandArgument[]
				{
					new(false, new[] { UserCommandArgument.Type.String }, typeof(UserCommandInfo), "command", new SimpleModelAdditionalInfoProvider(dic))
				},
				new SimpleModelAdditionalInfoProvider((Localizer, typeof(IStringLocalizer)),
					(new CommandDescription("ShowsCmdInfo", "ShowsCmdInfo", LaunchGroup.Everyone, null, null, "Help"), typeof(CommandDescription))));
			}
		}
	}
}
