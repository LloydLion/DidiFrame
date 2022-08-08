using TestBot.Systems.Parties.CommandEvironment;
using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.Utils;

namespace TestBot.Systems.Parties
{
	internal class CommandsHandler : ICommandsModule
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


		[Command("party create")]
		public UserCommandResult CreateParty(UserCommandContext ctx, [Validator(typeof(NoPartyExist))] string name, [Validator(typeof(ForeachValidator), typeof(NoBot))][Validator(typeof(ForeachValidator), typeof(NoInvoker))] IMember[] members)
		{
			systemCore.CreateParty(name, ctx.SendData.Invoker, members);
			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["PartyCreated"]));
		}

		[Command("party rename")]
		public UserCommandResult RenameParty(UserCommandContext _, [Validator(typeof(InvokerIsPartyOwner))] IObjectController<PartyModel> party, [Validator(typeof(NoPartyExist))] string newName)
		{
			using var holder = party.Open();
			holder.Object.Name = newName;

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["PartyRenamed"]));
		}

		[Command("party join")]
		public UserCommandResult JoinIntoParty(UserCommandContext _, [Validator(typeof(InvokerIsPartyOwner))] IObjectController<PartyModel> party, [Validator(typeof(NoInvoker))][Validator(typeof(MemberInParty), "party", true)] IMember member)
		{
			using var holder = party.Open();
			holder.Object.Members.Add(member);

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["MemberJoined"]));
		}

		[Command("party kick")]
		public UserCommandResult KickFromParty(UserCommandContext _, [Validator(typeof(InvokerIsPartyOwner))] IObjectController<PartyModel> party, [Validator(typeof(NoInvoker))][Validator(typeof(MemberInParty), "party", false)] IMember member)
		{
			using var holder = party.Open();
			holder.Object.Members.Remove(member);

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["MemberKicked"]));
		}

		[Command("party my")]
		public UserCommandResult ShowMyParties(UserCommandContext ctx)
		{
			using var parties = systemCore.GetPartiesWith(ctx.SendData.Invoker).Open();
			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, uiHelper.CreatePersonalTablet(parties.Object, ctx.SendData.Invoker));
		}

		[Command("party info")]
		public UserCommandResult ShowPartyInfo(UserCommandContext _, IObjectController<PartyModel> party)
		{
			using var holder = party.Open();
			var report = uiHelper.CreatePartyTablet(holder.Object);
			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, report);
		}
	}
}
