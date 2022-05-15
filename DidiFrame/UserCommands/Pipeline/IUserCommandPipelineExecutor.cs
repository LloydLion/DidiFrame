namespace DidiFrame.UserCommands.Pipeline
{
	public interface IUserCommandPipelineExecutor
	{
		public Task<UserCommandResult?> ProcessAsync(UserCommandPipeline pipeline, object input, UserCommandSendData sendData);
	}
}
