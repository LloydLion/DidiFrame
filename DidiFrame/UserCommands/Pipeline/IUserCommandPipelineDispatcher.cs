namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Callback for user command pipeline dispatcher
	/// </summary>
	/// <typeparam name="TOut">Type of dispatcher's output</typeparam>
	/// <param name="invoker">Dispathcer that calls it</param>
	/// <param name="dispatcherOutput">Dispacher's output</param>
	/// <param name="sendData">Send data of commnad</param>
	/// <param name="stateObject">Special state object to call other dispatcher's methods</param>
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

		/// <summary>
		/// Responds to command call with user command result
		/// </summary>
		/// <param name="stateObject">Special state object that have been given by dispatcher</param>
		/// <param name="result">Result of user command pipeline</param>
		public void Respond(object stateObject, UserCommandResult result);

		/// <summary>
		/// Finalizes pipeline
		/// </summary>
		/// <param name="stateObject">Special state object that have been given by dispatcher</param>
		public void FinalizePipeline(object stateObject);
	}
}
