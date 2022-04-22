using CGZBot3.UserCommands;

namespace CGZBot3.UserCommands
{
	public interface IUserCommandsHandler
	{
		public Task HandleAsync(UserCommandPreContext ctx, Action<UserCommandResult> callback);
	}
}
