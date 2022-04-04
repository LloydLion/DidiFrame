using CGZBot3.Entities.Message;
using CGZBot3.Systems.Parties.Validators;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation.Validators;
using CGZBot3.UserCommands.Loader.Reflection;

namespace CGZBot3.Systems.Parties
{
	internal class CommandsHandler : ICommandsHandler
	{
		private readonly ISystemCore systemCore;
		private readonly IStringLocalizer<CommandsHandler> localizer;
		private readonly UIHelper uiHelper;


		public CommandsHandler(ISystemCore systemCore, IStringLocalizer<CommandsHandler> localizer, UIHelper uiHelper)
		{
			this.systemCore = systemCore;
			this.localizer = localizer;
			this.uiHelper = uiHelper;
		}


		[Command("party")]
		public UserCommandResult CreateParty(UserCommandContext ctx, [Validator(typeof(PartyExist), true)] string name, [Validator(typeof(ForeachValidator), typeof(NoBot))][Validator(typeof(ForeachValidator), typeof(NoInvoker))] IMember[] members)
		{
			systemCore.CreateParty(name, ctx.Invoker, members);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["PartyCreated"]) };
		}

		[Command("renameparty")]
		public UserCommandResult RenameParty(UserCommandContext ctx, [Validator(typeof(PartyExistAndInvokerIsOwner))] string name, [Validator(typeof(PartyExist), true)] string newName)
		{
			var value = systemCore.GetParty(ctx.Invoker.Server, name);
			value.Object.Name = newName;
			value.Dispose();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["PartyRenamed"]) };
		}

		[Command("joinparty")]
		public UserCommandResult JoinIntoParty(UserCommandContext ctx, [Validator(typeof(PartyExistAndInvokerIsOwner))] string name, [Validator(typeof(NoInvoker))][Validator(typeof(MemberInParty), "name", true)] IMember member)
		{
			var value = systemCore.GetParty(ctx.Invoker.Server, name);
			value.Object.Members.Add(member);
			value.Dispose();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["MemberJoined"]) };
		}

		[Command("kickparty")]
		public UserCommandResult KickFromParty(UserCommandContext ctx, [Validator(typeof(PartyExistAndInvokerIsOwner))] string name, [Validator(typeof(NoInvoker))][Validator(typeof(MemberInParty), "name", false)] IMember member)
		{
			var value = systemCore.GetParty(ctx.Invoker.Server, name);
			using (value) value.Object.Members.Remove(member);
			value.Dispose();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["MemberKicked"]) };
		}

		[Command("myparties")]
		public UserCommandResult ShowMyParties(UserCommandContext ctx)
		{
			using var parties = systemCore.GetPartiesWith(ctx.Invoker);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = uiHelper.CreatePersonalTablet(parties.Object, ctx.Invoker) };
		}

		[Command("partyinfo")]
		public UserCommandResult ShowPartyInfo(UserCommandContext ctx, [Validator(typeof(PartyExist), false)] string name)
		{
			using var value = systemCore.GetParty(ctx.Invoker.Server, name);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = uiHelper.CreatePartyTablet(value.Object) };
		}
	}
}
