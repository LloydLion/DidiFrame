using DidiFrame;
using DidiFrame.Entities.Message;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ArgumentsValidation.Validators;
using DidiFrame.UserCommands.InvokerFiltartion.Filters;
using DidiFrame.UserCommands.Loader.Reflection;

namespace CGZBot3.Systems.Reputation
{
	internal class CommandsHandler : ICommandsHandler
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

			await member.SendDirectMessageAsync(uiHelper.CreateDirectNotification(ctx.Invoker, amount, reason.JoinWords()));

			if (doMsg) return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["IllegalAppliedWarning", member.UserName, amount]) };
			else return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["IllegalApplied", member.UserName, amount]) };
		}

		[Command("myrp")]
		public Task<UserCommandResult> SeeReputaion(UserCommandContext ctx)
		{
			var rp = system.GetReputation(ctx.Invoker);
			var sm = uiHelper.CreateReputaionTablet(rp, ctx.Invoker);
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = sm });
		}

		[Command("seerp")]
		public Task<UserCommandResult> SeeReputaion(UserCommandContext _, [Validator(typeof(NoBot))] IMember member)
		{
			var rp = system.GetReputation(member);
			var sm = uiHelper.CreateReputaionTablet(rp, member);
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = sm });
		}
	}
}
