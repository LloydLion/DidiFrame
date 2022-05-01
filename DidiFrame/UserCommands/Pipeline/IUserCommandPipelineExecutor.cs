namespace DidiFrame.UserCommands.Pipeline
{
	public interface IUserCommandPipelineExecutor
	{
		public UserCommandResult? Process(object input, UserCommandSendData sendData);
	}
}
