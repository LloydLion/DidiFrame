using CGZBot3.Entities.Message;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.Loader;

namespace CGZBot3.Systems.Reputation
{
	internal class CommandsHandler
	{
		private readonly SystemCore system;
		private readonly UIHelper uiHelper;
		private readonly IValidator<MemberLegalLevelChangeOperationArgs> validator;
		private readonly IStringLocalizer<CommandsHandler> localizer;


		public CommandsHandler(
			SystemCore system,
			UIHelper uiHelper,
			IValidator<MemberLegalLevelChangeOperationArgs> validator,
			IStringLocalizer<CommandsHandler> localizer)
		{
			this.system = system;
			this.uiHelper = uiHelper;
			this.validator = validator;
			this.localizer = localizer;
		}


		[Command("illegal")]
		public async Task<UserCommandResult> AddIllegal(UserCommandContext ctx, IMember member, int amount, string[] reason)
		{
			if (!member.HasPermissionIn(Permissions.KickMembers, ctx.Channel))
				return new UserCommandResult(UserCommandCode.NoPermission) { RespondMessage = new MessageSendModel(localizer["IllegalNoHasPermission", Permissions.KickMembers]) };


			var args = new MemberLegalLevelChangeOperationArgs(member, amount);
			var vres = validator.Validate(args);

			if (!vres.IsValid)
				return new UserCommandResult(UserCommandCode.InvalidInput) { RespondMessage = new MessageSendModel(localizer["IllegalInvalidInput", vres.Errors[0]]) };

			var doMsg = system.AddIllegal(args);

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
		public Task<UserCommandResult> SeeReputaion(UserCommandContext _, IMember member)
		{
			var rp = system.GetReputation(member);
			var sm = uiHelper.CreateReputaionTablet(rp, member);
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = sm });
		}
	}
}
