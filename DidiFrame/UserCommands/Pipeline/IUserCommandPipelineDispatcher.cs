namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Dispatcher for user command pipeline, it is origin and end of pipeline
	/// </summary>
	/// <typeparam name="TOut">Type of origin type of pipeline</typeparam>
	public interface IUserCommandPipelineDispatcher<out TOut> where TOut : notnull
	{
		/// <summary>
		/// Subscribes sync hander to start-pipeline event
		/// </summary>
		/// <param name="actionWithCallback">Handler that recives start object, send data and callback to call at end</param>
		public void SetSyncCallback(Action<TOut, UserCommandSendData, Action<UserCommandResult>> actionWithCallback);
	}
}
