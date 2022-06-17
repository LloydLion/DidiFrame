namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Executor for user command pipeline
	/// </summary>
	public interface IUserCommandPipelineExecutor
	{
		/// <summary>
		/// Processes data from dispatcher to ready result using pipeline model
		/// </summary>
		/// <param name="pipeline">Pipeline model itself</param>
		/// <param name="input">Input object from dispatcher</param>
		/// <param name="sendData">Send data from dispatcher</param>
		/// <returns>Task with result for dispatcher or null if pipeline has dropped</returns>
		public Task<UserCommandResult?> ProcessAsync(UserCommandPipeline pipeline, object input, UserCommandSendData sendData, object dispatcherState);
	}
}
