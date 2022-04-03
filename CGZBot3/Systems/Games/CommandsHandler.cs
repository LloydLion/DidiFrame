using CGZBot3.Entities.Message;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.Loader.Reflection;

namespace CGZBot3.Systems.Games
{
	internal class CommandsHandler : ICommandsHandler
	{
		private readonly ISystemCore systemCore;
		private readonly IStringLocalizer<CommandsHandler> localizer;


		public CommandsHandler(ISystemCore systemCore, IStringLocalizer<CommandsHandler> localizer)
		{
			this.systemCore = systemCore;
			this.localizer = localizer;
		}


		[Command("tgame")]
		public UserCommandResult CreateGame(UserCommandContext ctx, string name, string description, string startAtMembersPre, IMember[] invited)
		{
			bool req = true;

			if (startAtMembersPre.StartsWith("req")) startAtMembersPre = startAtMembersPre[3..];
			else req = false;

			if (int.TryParse(startAtMembersPre, out int startAtMembers) == false) return new UserCommandResult(UserCommandCode.InvalidInputFormat) { RespondMessage = new MessageSendModel(localizer["InvalidStartAtMembersInput"]) };

			systemCore.CreateGame(ctx.Invoker, name, req, description, invited, startAtMembers);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameCreated"]) };
		}

		[Command("reinvitegame")]
		public UserCommandResult ChangeInvitesGame(UserCommandContext ctx, string name, IMember[] invited)
		{
			if (systemCore.TryGetGame(ctx.Invoker, name, out var value))
			{
				using var baseObj = value?.GetBase() ?? throw new ImpossibleVariantException();
				baseObj.Object.Invited.Clear();
				foreach (var item in invited) baseObj.Object.Invited.Add(item);

				return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameInvitesChanged"]) };
			}
			else return new UserCommandResult(UserCommandCode.OtherUserError) { RespondMessage = new MessageSendModel(localizer["GameNotFound"]) };
		}

		[Command("renamegame")]
		public UserCommandResult CreateGame(UserCommandContext ctx, string name, string newName)
		{
			if (systemCore.TryGetGame(ctx.Invoker, name, out var value))
			{
				using var baseObj = value?.GetBase() ?? throw new ImpossibleVariantException();
				baseObj.Object.Name = newName;

				return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameRenamed"]) };
			}
			else return new UserCommandResult(UserCommandCode.OtherUserError) { RespondMessage = new MessageSendModel(localizer["GameNotFound"]) };
		}

		[Command("redescribegame")]
		public UserCommandResult ChangeDescriptionGame(UserCommandContext ctx, string name, string newDescription)
		{
			if (systemCore.TryGetGame(ctx.Invoker, name, out var value))
			{
				using var baseObj = value?.GetBase() ?? throw new ImpossibleVariantException();
				baseObj.Object.Description = newDescription;

				return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameDescriptionChanged"]) };
			}
			else return new UserCommandResult(UserCommandCode.OtherUserError) { RespondMessage = new MessageSendModel(localizer["GameNotFound"]) };
		}

		[Command("changegamecond")]
		public UserCommandResult ChangeGameStartCondition(UserCommandContext ctx, string name, string startAtMembersPre)
		{
			bool req = true;

			if (startAtMembersPre.StartsWith("req")) startAtMembersPre = startAtMembersPre[3..];
			else req = false;

			if (int.TryParse(startAtMembersPre, out int startAtMembers) == false) return new UserCommandResult(UserCommandCode.InvalidInputFormat) { RespondMessage = new MessageSendModel(localizer["InvalidStartAtMembersInput"]) };

			if (systemCore.TryGetGame(ctx.Invoker, name, out var value))
			{
				using var baseObj = value?.GetBase() ?? throw new ImpossibleVariantException();
				baseObj.Object.WaitEveryoneInvited = req;
				baseObj.Object.StartAtMembers = startAtMembers;

				return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameStartConditionChanged"]) };
			}
			else return new UserCommandResult(UserCommandCode.OtherUserError) { RespondMessage = new MessageSendModel(localizer["GameNotFound"]) };
		}
	}
}
