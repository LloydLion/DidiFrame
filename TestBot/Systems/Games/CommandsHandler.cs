using TestBot.Systems.Games.CommandEvironment;
using TestBot.Systems.Parties;
using TestBot.Systems.Parties.CommandEvironment;
using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.Utils;

namespace TestBot.Systems.Games
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


		[Command("game create")]
		public UserCommandResult CreateGame(UserCommandContext ctx,
			[Validator(typeof(NormalString))][Validator(typeof(NoGameExist))] string name,
			[Validator(typeof(NormalString))] string description,
			[Validator(typeof(ValidStartAtMembersString))] string startAtMembersPre,
			[Validator(typeof(ForeachValidator), typeof(NoInvoker))][Validator(typeof(ForeachValidator), typeof(NoBot))] IMember[] invited)
		{
			bool req = true;
			if (startAtMembersPre.StartsWith("req")) startAtMembersPre = startAtMembersPre[3..];
			else req = false;
			var startAtMembers = int.Parse(startAtMembersPre);

			systemCore.CreateGame(ctx.SendData.Invoker, name, req, description, invited, startAtMembers);

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["GameCreated"]));
		}

		[Command("game invite-members")]
		public UserCommandResult InviteMembersToGame(UserCommandContext _, GameLifetime game,
			[Validator(typeof(ForeachValidator), typeof(NoInvoker))][Validator(typeof(ForeachValidator), typeof(NoBot))] IMember[] invited)
		{
			game.Invite(invited);

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["MembersHaveInvitedToGame"]));
		}

		[Command("game invite-party")]
		public UserCommandResult InvitePartyToGame(UserCommandContext _, GameLifetime game,
			[Validator(typeof(NoPartyExist))] IObjectController<PartyModel> party)
		{
			using var holder = party.Open();

			game.Invite(holder.Object.Members.Append(holder.Object.Creator).ToArray());

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["PartyHasInvitedToGame"]));
		}

		[Command("game clear-invites")]
		public UserCommandResult ClearGameInvite(UserCommandContext _, GameLifetime game)
		{
			game.ClearInvites();

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["GameInvitesHaveCleared"]));
		}

		[Command("game rename")]
		public UserCommandResult RenameGame(UserCommandContext _, GameLifetime game,
			[Validator(typeof(NormalString))][Validator(typeof(NoGameExist))] string newName)
		{
			game.Rename(newName);

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["GameHasRenamed"]));
		}

		[Command("game redescribe")]
		public UserCommandResult ChangeDescriptionGame(UserCommandContext _, GameLifetime game,
			[Validator(typeof(NormalString))] string newDescription)
		{
			game.ChangeDescription(newDescription);

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["GameDescriptionHasChanged"]));
		}

		[Command("game change-cond")]
		public UserCommandResult ChangeGameStartCondition(UserCommandContext _, GameLifetime game,
			[Validator(typeof(ValidStartAtMembersString))] string startAtMembersPre)
		{
			bool req = true;
			if (startAtMembersPre.StartsWith("req")) startAtMembersPre = startAtMembersPre[3..];
			else req = false;
			var startAtMembers = int.Parse(startAtMembersPre);

			game.ChangeStartCondition(startAtMembers, req);

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["GameStartConditionHasChanged"]));
		}

		[Command("game cancel")]
		public UserCommandResult CancelGame(UserCommandContext _, GameLifetime game)
		{
			game.CloseAsync();

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["GameHasCanceled"]));
		}

		[Command("game send-invites")]
		public async Task<UserCommandResult> SendGameInvites(UserCommandContext _, GameLifetime game)
		{
			var tasks = new List<Task>();
			foreach (var member in game.GetInvited())
			{
				tasks.Add(member.SendDirectMessageAsync(uiHelper.CreateInvitationTablet(game.GetCreator(), game.GetName())));
			}

			await Task.WhenAll(tasks);

			return UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new MessageSendModel(localizer["InvitationHaveSent"]));
		}
	}
}
