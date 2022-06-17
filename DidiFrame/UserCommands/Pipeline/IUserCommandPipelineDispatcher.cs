namespace DidiFrame.UserCommands.Pipeline
{
	public delegate void DispatcherSyncCallback<in TOut>(IUserCommandPipelineDispatcher<TOut> invoker,
		TOut dispatcherOutput, UserCommandSendData sendData, object stateObject) where TOut : notnull;


	/// <summary>
	/// Dispatcher for user command pipeline, it is origin and end of pipeline
	/// </summary>
	/// <typeparam name="TOut">Type of origin type of pipeline</typeparam>
	public interface IUserCommandPipelineDispatcher<out TOut> where TOut : notnull
	{
		/// <summary>
		/// Subscribes sync hander to start-pipeline event
		/// </summary>
		/// <param name="callback">Handler that recives start object, send data and state object to finalize pipeline using dispathcer</param>
		public void SetSyncCallback(DispatcherSyncCallback<TOut> callback);

		public void Respond(object stateObject, UserCommandResult result);

		public void FinalizePipeline(object stateObject);
	}
}
