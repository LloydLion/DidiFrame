using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;

namespace DidiFrame.Testing.UserCommands
{
	internal class NullDispatcher<T> : IUserCommandPipelineDispatcher<T> where T : notnull
	{
		private DispatcherCallback<T>? callback;
		private TaskCompletionSource<ExecutionStatus>? taskSource;


		public void FinalizePipeline(object stateObject)
		{
			if (taskSource is null) throw new InvalidOperationException("This method was called outside pipeline");
			taskSource.SetResult(new ExecutionStatus(((State)stateObject).Responces));
			taskSource = null;
		}

		public Task RespondAsync(object stateObject, UserCommandResult result)
		{
			if (taskSource is null) throw new InvalidOperationException("This method was called outside pipeline");
			var state = (State)stateObject;
			state.Responces.Add(result);
			return Task.CompletedTask;
		}

		public void SetSyncCallback(DispatcherCallback<T> callback)
		{
			this.callback = callback;
		}

		public Task<ExecutionStatus> Run(T output, UserCommandSendData sendData)
		{
			if (callback is null)
				throw new NullReferenceException("Callback was null");
			if (taskSource is not null) throw new InvalidOperationException("This method was called while pipeline is running");
			taskSource = new();
			callback.Invoke(this, output, sendData, new State());
			return taskSource.Task;
		}


		public class ExecutionStatus
		{
			public IReadOnlyList<UserCommandResult> Responces { get; }


			public ExecutionStatus(IReadOnlyList<UserCommandResult> responces)
			{
				Responces = responces;
			}
		}

		private sealed class State
		{
			public List<UserCommandResult> Responces { get; } = new();
		}
	}
}
