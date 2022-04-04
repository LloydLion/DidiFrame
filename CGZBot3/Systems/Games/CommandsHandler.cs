﻿using CGZBot3.Entities.Message;
using CGZBot3.Systems.Games.Validators;
using CGZBot3.Systems.Parties.Validators;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation.Validators;
using CGZBot3.UserCommands.Loader.Reflection;
using PartiesISystemCore = CGZBot3.Systems.Parties.ISystemCore;

namespace CGZBot3.Systems.Games
{
	internal class CommandsHandler : ICommandsHandler
	{
		private readonly PartiesISystemCore partiesSystem;
		private readonly ISystemCore systemCore;
		private readonly IStringLocalizer<CommandsHandler> localizer;


		public CommandsHandler(PartiesISystemCore partiesSystem, ISystemCore systemCore, IStringLocalizer<CommandsHandler> localizer)
		{
			this.partiesSystem = partiesSystem;
			this.systemCore = systemCore;
			this.localizer = localizer;
		}


		[Command("game create")]
		public UserCommandResult CreateGame(UserCommandContext ctx,
			[Validator(typeof(NormalString))][Validator(typeof(GameExistAndInvokerIsOwner), true)] string name,
			[Validator(typeof(NormalString))] string description,
			string startAtMembersPre,
			[Validator(typeof(ForeachValidator), typeof(NoInvoker))][Validator(typeof(ForeachValidator), typeof(NoBot))] IMember[] invited)
		{
			bool req = true;

			if (startAtMembersPre.StartsWith("req")) startAtMembersPre = startAtMembersPre[3..];
			else req = false;

			if (int.TryParse(startAtMembersPre, out int startAtMembers) == false) return new UserCommandResult(UserCommandCode.InvalidInputFormat) { RespondMessage = new MessageSendModel(localizer["InvalidStartAtMembersInput"]) };

			systemCore.CreateGame(ctx.Invoker, name, req, description, invited, startAtMembers);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameCreated"]) };
		}

		[Command("game invite-members")]
		public UserCommandResult InviteMembersToGame(UserCommandContext ctx,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] string name,
			[Validator(typeof(ForeachValidator), typeof(NoInvoker))][Validator(typeof(ForeachValidator), typeof(NoBot))] IMember[] invited)
		{
			var game = systemCore.GetGame(ctx.Invoker, name);
			using var baseObj = game.GetBase();

			foreach (var item in invited) if (baseObj.Object.Invited.Contains(item) == false) baseObj.Object.Invited.Add(item);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["MembersHaveInvitedToGame"]) };
		}

		[Command("game invite-party")]
		public UserCommandResult InvitePartyToGame(UserCommandContext ctx,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] string name,
			[Validator(typeof(PartyExist), false)] string partyName)
		{
			var game = systemCore.GetGame(ctx.Invoker, name);
			using var baseObj = game.GetBase();

			using var party = partiesSystem.GetParty(ctx.Invoker.Server, partyName);

			foreach (var item in party.Object.Members) if (baseObj.Object.Invited.Contains(item) == false) baseObj.Object.Invited.Add(item);

			if (baseObj.Object.Invited.Contains(party.Object.Creator) == false && baseObj.Object.Creator != party.Object.Creator)
				baseObj.Object.Invited.Add(party.Object.Creator);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["PartyHasInvitedToGame"]) };
		}

		[Command("game clear-invites")]
		public UserCommandResult ClearGameInvite(UserCommandContext ctx,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] string name)
		{
			var game = systemCore.GetGame(ctx.Invoker, name);
			using var baseObj = game.GetBase();

			baseObj.Object.Invited.Clear();

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameInvitesHaveCleared"]) };
		}

		[Command("game rename")]
		public UserCommandResult RenameGame(UserCommandContext ctx,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] string name,
			[Validator(typeof(NormalString))][Validator(typeof(GameExistAndInvokerIsOwner), true)] string newName)
		{
			var game = systemCore.GetGame(ctx.Invoker, name);
			using var baseObj = game.GetBase();

			baseObj.Object.Name = newName;

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameRenamed"]) };
		}

		[Command("game redescribe")]
		public UserCommandResult ChangeDescriptionGame(UserCommandContext ctx,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] string name,
			[Validator(typeof(NormalString))] string newDescription)
		{
			var game = systemCore.GetGame(ctx.Invoker, name);
			using var baseObj = game.GetBase();

			baseObj.Object.Description = newDescription;

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameDescriptionChanged"]) };
		}

		[Command("game change-cond")]
		public UserCommandResult ChangeGameStartCondition(UserCommandContext ctx,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] string name,
			string startAtMembersPre)
		{
			bool req = true;

			if (startAtMembersPre.StartsWith("req")) startAtMembersPre = startAtMembersPre[3..];
			else req = false;

			if (int.TryParse(startAtMembersPre, out int startAtMembers) == false) return new UserCommandResult(UserCommandCode.InvalidInputFormat) { RespondMessage = new MessageSendModel(localizer["InvalidStartAtMembersInput"]) };

			var game = systemCore.GetGame(ctx.Invoker, name);
			using var baseObj = game.GetBase();

			baseObj.Object.WaitEveryoneInvited = req;
			baseObj.Object.StartAtMembers = startAtMembers;

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameStartConditionChanged"]) };
		}

		[Command("game cancel")]
		public UserCommandResult CancelGame(UserCommandContext ctx,
			[Validator(typeof(GameExistAndInvokerIsOwner), false)] string name)
		{
			systemCore.CancelGame(ctx.Invoker, name);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameCanceled"]) };
		}
	}
}
