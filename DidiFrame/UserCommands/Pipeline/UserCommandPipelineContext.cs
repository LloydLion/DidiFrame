namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Context for item that executing in user command pipeline
	/// </summary>
	public class UserCommandPipelineContext
	{
		private readonly Action<UserCommandResult> sendResponce;


		/// <summary>
		/// Status of pipeline
		/// </summary>
		public Status CurrentStatus { get; private set; }

		/// <summary>
		/// Execution result if pipeline begins finalize. Not recomended use GetExecutionResult() method
		/// </summary>
		public UserCommandResult? ExecutionResult { get; private set; } = null;

		/// <summary>
		/// Send data from dispatcher
		/// </summary>
		public UserCommandSendData SendData { get; }

		/// <summary>
		/// Provided local services
		/// </summary>
		public IServiceProvider LocalServices { get; }


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.UserCommandPipelineContext
		/// </summary>
		/// <param name="localServices">Local services</param>
		/// <param name="sendData">Send data from dispatcher</param>
		public UserCommandPipelineContext(IServiceProvider localServices, UserCommandSendData sendData, Action<UserCommandResult> sendResponce)
		{
			LocalServices = localServices;
			SendData = sendData;
			this.sendResponce = sendResponce;
		}


		/// <summary>
		/// Sets status to BeginDrop
		/// </summary>
		/// <exception cref="InvalidOperationException">If pipeline begins finalize</exception>
		public void DropPipeline()
		{
			if (CurrentStatus != Status.ContinuePipeline)
				throw new InvalidOperationException("Enable to drop pipeline after finalizing");
			else CurrentStatus = Status.BeginDrop;
		}

		/// <summary>
		/// Sets status to BeginFinalize and command result
		/// </summary>
		/// <param name="result">Command result for dispatcher</param>
		/// <exception cref="InvalidOperationException">If pipeline begins drop</exception>
		public void FinalizePipeline(UserCommandResult result)
		{
			if (CurrentStatus != Status.ContinuePipeline)
				throw new InvalidOperationException("Enable to finalize pipeline after dropping");
			else
			{
				ExecutionResult = result;
				CurrentStatus = Status.BeginFinalize;
			}
		}

		/// <summary>
		/// Gets execution result if pipeline begins finalize
		/// </summary>
		/// <returns>Execution result</returns>
		/// <exception cref="InvalidOperationException">If pipeline doesn't begin finalize</exception>
		public UserCommandResult GetExecutionResult()
		{
			return ExecutionResult ?? throw new InvalidOperationException("No execution result if status is not BeginFinalize");
		}

		public void SendResponce(UserCommandResult result) => sendResponce(result);


		/// <summary>
		/// Represents a status of pipeline
		/// </summary>
		public enum Status
		{
			/// <summary>
			/// Pipeline works
			/// </summary>
			ContinuePipeline = default,
			/// <summary>
			/// Pipeline begins to be dropped
			/// </summary>
			BeginDrop,
			/// <summary>
			/// Pipeline begins to be finalized
			/// </summary>
			BeginFinalize,
		}
	}
}
