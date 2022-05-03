namespace DidiFrame.UserCommands.Pipeline
{
	public interface IUserCommandPipelineExecutor
	{
		public UserCommandResult? Process(UserCommandPipeline pipeline, object input, UserCommandSendData sendData);
	}
}
