namespace DidiFrame.UserCommands.Pipeline
{
	public class UserCommandPipelineContext
	{
		public Status CurrentStatus { get; private set; }

		public UserCommandResult? ExecutionResult { get; private set; } = null;

		public UserCommandSendData SendData { get; }


		public UserCommandPipelineContext(UserCommandSendData sendData)
		{
			SendData = sendData;
		}


		public void DropPipeline()
		{
			if (CurrentStatus != Status.ContinuePipeline)
				throw new InvalidOperationException("Enable to drop pipeline after finalizing");
			else CurrentStatus = Status.BeginDrop;
		}

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

		public UserCommandResult GetExecutionResult()
		{
			return ExecutionResult ?? throw new InvalidOperationException("No execution result if status is not BeginFinalize");
		}


		public enum Status
		{
			ContinuePipeline = default,
			BeginDrop,
			BeginFinalize,
		}
	}
}
