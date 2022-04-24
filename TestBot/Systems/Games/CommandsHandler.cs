using CGZBot3.Systems.Games.CommandEvironment;
using CGZBot3.Systems.Parties;
using CGZBot3.Systems.Parties.CommandEvironment;
using DidiFrame.Entities.Message;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ArgumentsValidation.Validators;
using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.Utils;

namespace CGZBot3.Systems.Games
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


		[Command("game create")]
		public UserCommandResult CreateGame(UserCommandContext ctx,
			[Validator(typeof(NormalString))][Validator(typeof(GameExistAndInvokerIsOwner), true)] string name,
			[Validator(typeof(NormalString))] string description,
			[Validator(typeof(ValidStartAtMembersString))] string startAtMembersPre,
			[Validator(typeof(ForeachValidator), typeof(NoInvoker))][Validator(typeof(ForeachValidator), typeof(NoBot))] IMember[] invited)
		{
			bool req = true;
			if (startAtMembersPre.StartsWith("req")) startAtMembersPre = startAtMembersPre[3..];
			else req = false;
			var startAtMembers = int.Parse(startAtMembersPre);

			systemCore.CreateGame(ctx.Invoker, name, req, description, invited, startAtMembers);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameCreated"]) };
		}

		[Command("game invite-members")]
		public UserCommandResult InviteMembersToGame(UserCommandContext _,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] GameLifetime game,
			[Validator(typeof(ForeachValidator), typeof(NoInvoker))][Validator(typeof(ForeachValidator), typeof(NoBot))] IMember[] invited)
		{
			game.Invite(invited);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["MembersHaveInvitedToGame"]) };
		}

		[Command("game invite-party")]
		public UserCommandResult InvitePartyToGame(UserCommandContext _,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] GameLifetime game,
			[Validator(typeof(PartyExist), false)] ObjectHolder<PartyModel> party)
		{
			game.Invite(party.Object.Members.Append(party.Object.Creator).ToArray());

			party.Dispose();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["PartyHasInvitedToGame"]) };
		}

		[Command("game clear-invites")]
		public UserCommandResult ClearGameInvite(UserCommandContext _,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] GameLifetime game)
		{
			game.ClearInvites();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameInvitesHaveCleared"]) };
		}

		[Command("game rename")]
		public UserCommandResult RenameGame(UserCommandContext _,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] GameLifetime game,
			[Validator(typeof(NormalString))][Validator(typeof(GameExistAndInvokerIsOwner), true)] string newName)
		{
			game.Rename(newName);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameHasRenamed"]) };
		}

		[Command("game redescribe")]
		public UserCommandResult ChangeDescriptionGame(UserCommandContext _,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] GameLifetime game,
			[Validator(typeof(NormalString))] string newDescription)
		{
			game.ChangeDescription(newDescription);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameDescriptionHasChanged"]) };
		}

		[Command("game change-cond")]
		public UserCommandResult ChangeGameStartCondition(UserCommandContext _,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] GameLifetime game,
			[Validator(typeof(ValidStartAtMembersString))] string startAtMembersPre)
		{
			bool req = true;
			if (startAtMembersPre.StartsWith("req")) startAtMembersPre = startAtMembersPre[3..];
			else req = false;
			var startAtMembers = int.Parse(startAtMembersPre);

			game.ChangeStartCondition(startAtMembers, req);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameStartConditionHasChanged"]) };
		}

		[Command("game cancel")]
		public UserCommandResult CancelGame(UserCommandContext _,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] GameLifetime game)
		{
			game.Close();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameHasCanceled"]) };
		}

		[Command("game send-invites")]
		public async Task<UserCommandResult> SendGameInvites(UserCommandContext _,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] GameLifetime game)
		{
			var baseObj = game.GetBaseClone();

			var tasks = new List<Task>();
			foreach (var member in baseObj.Invited)
			{
				tasks.Add(member.SendDirectMessageAsync(uiHelper.CreateInvitationTablet(baseObj.Creator, baseObj.Name)));
			}

			await Task.WhenAll(tasks);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["InvitationHaveSent"]) };
		}
	}
}
