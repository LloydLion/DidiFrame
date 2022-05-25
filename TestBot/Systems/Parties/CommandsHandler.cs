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
			systemCore.CreateParty(name, ctx.Invoker, members);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["PartyCreated"]) };
		}

		[Command("party rename")]
		public UserCommandResult RenameParty(UserCommandContext _, [Validator(typeof(InvokerIsPartyOwner))] ObjectHolder<PartyModel> party, [Validator(typeof(NoPartyExist))] string newName)
		{
			party.Object.Name = newName;
			party.Dispose();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["PartyRenamed"]) };
		}

		[Command("party join")]
		public UserCommandResult JoinIntoParty(UserCommandContext _, [Validator(typeof(InvokerIsPartyOwner))] ObjectHolder<PartyModel> party, [Validator(typeof(NoInvoker))][Validator(typeof(MemberInParty), "party", true)] IMember member)
		{
			party.Object.Members.Add(member);
			party.Dispose();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["MemberJoined"]) };
		}

		[Command("party kick")]
		public UserCommandResult KickFromParty(UserCommandContext _, [Validator(typeof(InvokerIsPartyOwner))] ObjectHolder<PartyModel> party, [Validator(typeof(NoInvoker))][Validator(typeof(MemberInParty), "party", false)] IMember member)
		{
			party.Object.Members.Remove(member);
			party.Dispose();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["MemberKicked"]) };
		}

		[Command("party my")]
		public UserCommandResult ShowMyParties(UserCommandContext ctx)
		{
			using var parties = systemCore.GetPartiesWith(ctx.Invoker);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = uiHelper.CreatePersonalTablet(parties.Object, ctx.Invoker) };
		}

		[Command("party info")]
		public UserCommandResult ShowPartyInfo(UserCommandContext _, ObjectHolder<PartyModel> party)
		{
			var report = uiHelper.CreatePartyTablet(party.Object);
			party.Dispose();
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = report };
		}
	}
}
