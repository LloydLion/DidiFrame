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
		public Task<UserCommandResult> CreateGame(UserCommandContext ctx, string name, string description, string startAtMembersPre, IMember[] invited)
		{
			bool req = true;

			if (startAtMembersPre.StartsWith("req")) startAtMembersPre = startAtMembersPre[3..];
			else req = false;

			if (int.TryParse(startAtMembersPre, out int startAtMembers) == false) return Task.FromResult(new UserCommandResult(UserCommandCode.InvalidInputFormat) { RespondMessage = new MessageSendModel(localizer["InvalidStartAtMembersInput"]) });

			systemCore.CreateGame(ctx.Invoker, name, req, description, invited, startAtMembers);

			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["GameCreated"]) });
		}
	}
}
