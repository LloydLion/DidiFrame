using System.Diagnostics;

namespace CGZBot3.Utils.StateMachine
{
	internal class TaskTransitWorker<TState> : AbstractTransitWorker<TState> where TState : struct
	{
		private static readonly EventId TaskErrorID = new(12, "TaskError");


		private readonly Func<CancellationToken, Task> taskFactory;
		private readonly CancellationTokenSource source = new();
		private Task? currentTask;


		public TaskTransitWorker(TState activation, Nullable<TState> destination, Func<CancellationToken, Task> taskFactory)
			: base(activation, destination)
		{
			this.taskFactory = taskFactory;
		}


		public override void Activate() => currentTask = taskFactory(source.Token);

		public override bool CanDoTransit() => currentTask?.IsCompleted ?? throw new ImpossibleVariantException();

		public override void Disactivate()
		{
			source.Cancel();

			if (currentTask is null) return;

			var sw = new Stopwatch();
			sw.Start();
				
			while(!currentTask.IsCompleted && sw.ElapsedMilliseconds <= 3500)
			{
				Thread.Sleep(300);
			}

			sw.Stop();

			if (currentTask.IsCompleted == false)
			{
				StateMachine?.Logger.Log(LogLevel.Warning, TaskErrorID, "Observable task too long execute after cancel. Task released! and will not control by worker");
			}
			else
			{
				if (currentTask.Exception is not null)
					StateMachine?.Logger.Log(LogLevel.Warning, TaskErrorID, currentTask.Exception, "Observable task finished with exception");
			}

			currentTask = null;
		}
	}
}
