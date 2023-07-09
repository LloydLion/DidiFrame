namespace DidiFrame.Utils
{
	public sealed class TaskBasedServerTask : IServerTask
	{
		private readonly Task task;
		private readonly CancellationTokenSource? token;
		private Action? callback = null;


		public TaskBasedServerTask(Task task, CancellationTokenSource token)
		{
			this.task = task;
			this.token = token;
		}

		public TaskBasedServerTask(Task task)
		{
			this.task = task;
		}


		public void Execute(IServerTaskExecutionContext context)
		{
			task.ContinueWith((_) =>
			{
				context.PostAction(() =>
				{
					callback?.Invoke();
				});
			});
		}

		public IServerTaskObserver GetObserver()
		{
			return new Observer(this);
		}

		public void PerformTerminate()
		{
			token?.Cancel();
		}

		private sealed class Observer : IServerTaskObserver
		{
			private readonly TaskBasedServerTask owner;


			public Observer(TaskBasedServerTask owner)
			{
				this.owner = owner;
			}


			public void OnCompleted(Action continuation)
			{
				owner.callback = continuation;
			}
		}
	}
}
