namespace DidiFrame.UserCommands.Pipeline
{
	public interface IUserCommandPipelineExecutor
	{
		public UserCommandResult? Process<TInput>(TInput input, UserCommandSendData sendData) where TInput : notnull;
	}
}
