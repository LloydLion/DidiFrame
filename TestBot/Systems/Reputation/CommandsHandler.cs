using DidiFrame;
using DidiFrame.Entities.Message;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.Loader.Reflection;

namespace TestBot.Systems.Reputation
{
	internal class CommandsHandler : ICommandsModule
	{
		private readonly SystemCore system;
		private readonly UIHelper uiHelper;
		private readonly IStringLocalizer<CommandsHandler> localizer;


		public CommandsHandler(
			SystemCore system,
			UIHelper uiHelper,
			IStringLocalizer<CommandsHandler> localizer)
		{
			this.system = system;
			this.uiHelper = uiHelper;
			this.localizer = localizer;
		}


		[Command("illegal")]
		[InvokerFilter(typeof(PermissionFilter), Permissions.KickMembers)]
		public async Task<UserCommandResult> AddIllegal(UserCommandContext ctx, [Validator(typeof(NoBot))] IMember member, [Validator(typeof(GreaterThen), 0)] int amount, string[] reason)
		{
			var doMsg = system.AddIllegal(new MemberLegalLevelChangeOperationArgs(member, amount));

			await member.SendDirectMessageAsync(uiHelper.CreateDirectNotification(ctx.SendData.Invoker, amount, reason.JoinWords()));

			if (doMsg) return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["IllegalAppliedWarning", member.UserName, amount]));
			else return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["IllegalApplied", member.UserName, amount]));
		}

		[Command("myrp")]
		public UserCommandResult SeeReputaion(UserCommandContext ctx)
		{
			var rp = system.GetReputation(ctx.SendData.Invoker);
			var sm = uiHelper.CreateReputaionTablet(rp, ctx.SendData.Invoker);
			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, sm);
		}

		[Command("seerp")]
		public UserCommandResult SeeReputaion(UserCommandContext _, [Validator(typeof(NoBot))] IMember member)
		{
			var rp = system.GetReputation(member);
			var sm = uiHelper.CreateReputaionTablet(rp, member);
			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, sm);
		}
	}
}
