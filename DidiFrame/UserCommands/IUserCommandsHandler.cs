using DidiFrame.UserCommands;

namespace DidiFrame.UserCommands
{
	public interface IUserCommandsHandler
	{
		public Task HandleAsync(UserCommandPreContext ctx, Action<UserCommandResult> callback);
	}
}
