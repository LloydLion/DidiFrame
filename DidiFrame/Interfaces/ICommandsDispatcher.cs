using DidiFrame.UserCommands;

namespace DidiFrame.Interfaces
{
	public delegate void CommandWrittenHandler(UserCommandPreContext context, Action<UserCommandResult> callback);


	public interface ICommandsDispatcher
	{
		public event CommandWrittenHandler? CommandWritten;
	}
}
