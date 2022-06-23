using DidiFrame.UserCommands.ContextValidation.Arguments.Providers;
using DidiFrame.UserCommands.PreProcessing;
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


		private readonly IStringLocalizer<HelpCommandLoader> localizer;
		private readonly IUserCommandsRepository repository;
		private readonly IUserCommandContextConverter converter;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.EmbededCommands.Help.HelpCommandLoader
		/// </summary>
		/// <param name="localizer">Localizer for commands</param>
		/// <param name="repository">Commands repositroy to get commands</param>
		/// <param name="converter">Converter to get values provider from DidiFrame.UserCommands.Models.UserCommandInfo subconverter</param>
		public HelpCommandLoader(IStringLocalizer<HelpCommandLoader> localizer, IUserCommandsRepository repository, IUserCommandContextConverter converter)
		{
			this.localizer = localizer;
			this.repository = repository;
			this.converter = converter;
		}


		/// <inheritdoc/>
		public void LoadTo(IUserCommandsRepository rp)
		{
			var dic = new Dictionary<Type, object>();
			var provider = converter.GetSubConverter(typeof(UserCommandInfo)).CreatePossibleValuesProvider();
			if(provider is not null) dic.Add(typeof(IReadOnlyCollection<IUserCommandArgumentValuesProvider>), new[] { provider });
			dic.Add(typeof(ArgumentDescription), new ArgumentDescription("TargetCmd", null));

			rp.AddCommand(new UserCommandInfo("cmd", CmdHandler, new UserCommandArgument[]
			{
				new(false, new[] { UserCommandArgument.Type.String }, typeof(UserCommandInfo), "command", new SimpleModelAdditionalInfoProvider(dic))
			},
			new SimpleModelAdditionalInfoProvider((localizer, typeof(IStringLocalizer)),
				(new CommandDescription("ShowsCmdInfo", "ShowsCmdInfo", LaunchGroup.Everyone, null, null, "Help"), typeof(CommandDescription)))));

			//---------------------------

			dic = new Dictionary<Type, object>
			{
				{ typeof(IReadOnlyCollection<IUserCommandArgumentValuesProvider>), new IUserCommandArgumentValuesProvider[] { new Provider(rp) } },
				{ typeof(ArgumentDescription), new ArgumentDescription("TargetPage", null) }
			};

			rp.AddCommand(new UserCommandInfo("help", HelpHandler, new UserCommandArgument[]
			{
				new(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "page", new SimpleModelAdditionalInfoProvider(dic))
			}, new SimpleModelAdditionalInfoProvider((localizer, typeof(IStringLocalizer)),
				(new CommandDescription("ShowsCmdsList", "ShowsCmdsList", LaunchGroup.Everyone, null, null, "Help"), typeof(CommandDescription)))));
		}


		private Task<UserCommandResult> HelpHandler(UserCommandContext ctx)
		{
			var page = ctx.Arguments.Single().Value.As<int>();

			var cmds = repository.GetFullCommandList(ctx.Channel.Server);

			var embedBuilder = new MessageEmbedBuilder(localizer["HelpTitle"], localizer["HelpDescription"], new("#44dca5"));

			var error = Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new(localizer["NoPage"]) });
			if (page <= 0) return error;
			var data = cmds.Skip((page - 1) * MaxCmdsInOnePage).Take(MaxCmdsInOnePage);
			if (!data.Any()) return error;

			foreach (var cmd in data)
			{
				var desc = cmd.AdditionalInfo.GetExtension<CommandDescription>();
				var cmdloc = cmd.AdditionalInfo.GetExtension<IStringLocalizer>();

				if (desc is not null && cmdloc is not null) embedBuilder.AddField(new(localizer["HelpCommandName", cmd.Name], cmdloc[desc.ShortSpecify]));
				else embedBuilder.AddField(new(localizer["HelpCommandName", cmd.Name], localizer["NoDataProvided"]));
			}

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new() { MessageEmbeds = new[] { embedBuilder.Build() } } });
		}

		private Task<UserCommandResult> CmdHandler(UserCommandContext ctx)
		{
			var cmd = ctx.Arguments.Single().Value.As<UserCommandInfo>();

			var desc = cmd.AdditionalInfo.GetExtension<CommandDescription>();
			var cmdloc = cmd.AdditionalInfo.GetExtension<IStringLocalizer>();

			if (desc is null || cmdloc is null)
			{
				return Task.FromResult(new UserCommandResult(UserCommandCode.InternalError) { RespondMessage = new(localizer["NoDataProvided"]) });
			}


			var title = desc.DescribeGroup is null ? localizer["CmdTitle_1", cmdloc[cmd.Name]] : localizer["CmdTitle_2", cmdloc[cmd.Name], cmdloc[desc.DescribeGroup]];
			var embedBuilder = new MessageEmbedBuilder(title, localizer["CmdDescription", cmdloc[desc.Description]], new("#fc44a1"));

			//Execution group
			if (desc.LaunchGroup != LaunchGroup.Everyone || desc.LaunchDescription is not null)
			{
				string executionGroupContent;
				if (desc.LaunchDescription is null) executionGroupContent = localizer["ExecutionGroupContent_1", localizer["LaunchGroup:" + desc.LaunchGroup.ToString()]];
				else executionGroupContent = localizer["ExecutionGroupContent_2", localizer["LaunchGroup:" + desc.LaunchGroup.ToString()], cmdloc[desc.LaunchDescription]];
				embedBuilder.AddField(new(localizer["ExecutionGroupTitle"], executionGroupContent));
			}

			//Using
			var usingStr = $"{cmd.Name} " + string.Join(' ', cmd.Arguments.Select(s => s.IsArray ? $"[{s.Name} (Array of {s.OriginTypes.Single()})]..." : $"[{s.Name} ({string.Join(", ", s.OriginTypes)})]"));
			embedBuilder.AddField(new(localizer["UsingTitle"], usingStr));

			//Args desc
			foreach (var arg in cmd.Arguments)
			{
				var argDesc = arg.AdditionalInfo.GetExtension<ArgumentDescription>();
				embedBuilder.AddField(new EmbedField(localizer["ArgumentInfoTitle", arg.Name], argDesc is null ? localizer["NoDataProvided"] : cmdloc[argDesc.ShortSpecify]));
			}

			//Remarks
			var args = cmd.Arguments.Where(s => s.AdditionalInfo.GetExtension<ArgumentDescription>()?.Remarks is not null).ToArray();
			var rmStrs = args.Select(s => localizer["RemarkContent_2", s.Name, cmdloc[s.AdditionalInfo.GetRequiredExtension<ArgumentDescription>().Remarks ?? throw new ImpossibleVariantException()]]);
			var cmdRemark = desc.Remarks is null ? "" : '*' + localizer["RemarkContent_1", cmdloc[desc.Remarks]] + '\n';

			if (string.IsNullOrWhiteSpace(cmdRemark) == false || rmStrs.Any())
			{
				var readyRemarks = cmdRemark + '*' + string.Join("\n*", rmStrs);
				embedBuilder.AddField(new(localizer["RemarksTitle"], readyRemarks));
			}


			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new() { MessageEmbeds = new[] { embedBuilder.Build() } } });
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
				return (IReadOnlyCollection<object>)new Collection(max);
			}


			private class Collection : IReadOnlyCollection<int>
			{
				private readonly int max;


				public Collection(int max)
				{
					this.max = max;
				}


				public int Count => max + 1;


				public IEnumerator<int> GetEnumerator()
				{
					for (int i = 0; i <= max; i++) yield return i;
				}

				IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			}
		}
	}
}
