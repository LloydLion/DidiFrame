using DidiFrame.Utils.ExtendableModels;

namespace DidiFrame.UserCommands.Loader.EmbededCommands.Help
{
	/// <summary>
	/// Help and cmd commands loader
	/// </summary>
	public class HelpCommandLoader : IUserCommandsLoader
	{
		private readonly IStringLocalizer<HelpCommandLoader> localizer;
		private readonly IUserCommandsRepository repository;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.EmbededCommands.Help.HelpCommandLoader
		/// </summary>
		/// <param name="localizer">Localizer for commands</param>
		/// <param name="repository">Commands repositroy to get commands</param>
		public HelpCommandLoader(IStringLocalizer<HelpCommandLoader> localizer, IUserCommandsRepository repository)
		{
			this.localizer = localizer;
			this.repository = repository;
		}


		/// <inheritdoc/>
		public void LoadTo(IUserCommandsRepository rp)
		{
			rp.AddCommand(new UserCommandInfo("cmd", CmdHandler, new UserCommandArgument[]
			{
				new(false, new[] { UserCommandArgument.Type.String }, typeof(UserCommandInfo), "command",
					new SimpleModelAdditionalInfoProvider((new ArgumentDescription("TargetCmd", null), typeof(ArgumentDescription))))
			},
			new SimpleModelAdditionalInfoProvider((localizer, typeof(IStringLocalizer)),
				(new CommandDescription("ShowsCmdInfo", "ShowsCmdInfo", LaunchGroup.Everyone, null, null, "Help"), typeof(CommandDescription)))));

			rp.AddCommand(new UserCommandInfo("help", HelpHandler, new UserCommandArgument[]
			{
				new(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "page",
					new SimpleModelAdditionalInfoProvider((new ArgumentDescription("TargetPage", null), typeof(ArgumentDescription))))
			}, new SimpleModelAdditionalInfoProvider((localizer, typeof(IStringLocalizer)),
				(new CommandDescription("ShowsCmdsList", "ShowsCmdsList", LaunchGroup.Everyone, null, null, "Help"), typeof(CommandDescription)))));
		}


		private Task<UserCommandResult> HelpHandler(UserCommandContext ctx)
		{
			var page = ctx.Arguments.Single().Value.As<int>();

			var cmds = repository.GetCommandsFor(ctx.Channel.Server);

			var embedBuilder = new MessageEmbedBuilder(localizer["HelpTitle"], localizer["HelpDescription"], new("#44dca5"));

			var error = Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new(localizer["NoPage"]) });
			if (page <= 0) return error;
			var data = cmds.Skip((page - 1) * 20).Take(20);
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
	}
}
