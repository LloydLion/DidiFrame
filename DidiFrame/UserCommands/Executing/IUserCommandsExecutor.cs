using DidiFrame.UserCommands;

namespace DidiFrame.UserCommands.Executing
{
	public interface IUserCommandsExecutor
	{
		public Task HandleAsync(UserCommandPreContext ctx, Action<UserCommandResult> callback);
	}
}
