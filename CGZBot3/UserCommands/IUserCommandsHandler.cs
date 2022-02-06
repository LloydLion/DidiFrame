using CGZBot3.UserCommands;

namespace CGZBot3.UserCommands
{
	internal interface IUserCommandsHandler
	{
		public Task HandleAsync(UserCommandContext ctx);
	}
}
