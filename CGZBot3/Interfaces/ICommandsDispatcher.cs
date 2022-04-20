using CGZBot3.UserCommands;

namespace CGZBot3.Interfaces
{
	public delegate void CommandWrittenHandler(UserCommandPreContext context, Action<UserCommandResult> callback);


	public interface ICommandsDispatcher
	{
		public event CommandWrittenHandler? CommandWritten;
	}
}
