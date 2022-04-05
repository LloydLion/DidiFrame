using System.Diagnostics;

namespace CGZBot3.Utils.StateMachine
{
	internal class ResetTransitWorker<TState> : IStateTransitWorker<TState> where TState : struct
	{
		private static readonly EventId TaskErrorID = new(12, "TaskError");


		private readonly TState? tstate;
		private readonly Func<CancellationToken, Task> waitTaskFactory;
		private CancellationTokenSource cts = new();
		private Task? currentTask;
		private IStateMachine<TState>? stateMachine;


		public ResetTransitWorker(TState? tstate, Func<CancellationToken, Task> waitTaskFactory)
		{
			this.tstate = tstate;
			this.waitTaskFactory = waitTaskFactory;
		}


		public bool CanActivate(TState state) => true;

		public bool CanDoTransit() => currentTask?.IsCompleted ?? throw new ImpossibleVariantException();

		public void Disactivate()
		{
			cts.Cancel();

			if (currentTask is null) return;

			var sw = new Stopwatch();
			sw.Start();

			while (!currentTask.IsCompleted && sw.ElapsedMilliseconds <= 3500)
			{
				Thread.Sleep(300);
			}

			sw.Stop();

			if (currentTask.IsCompleted == false)
			{
				stateMachine?.Logger.Log(LogLevel.Warning, TaskErrorID, "Observable task too long execute after cancel. Task released! and will not control by worker");
			}
			else
			{
				if (currentTask.Exception is not null)
					stateMachine?.Logger.Log(LogLevel.Warning, TaskErrorID, currentTask.Exception, "Observable task finished with exception");
			}

			currentTask = null;
			cts = new();
		}

		public void Activate(IStateMachine<TState> stateMahcine)
		{
			stateMachine = stateMahcine;
			currentTask = waitTaskFactory(cts.Token);
		}

		public TState? DoTransit()
		{
			return tstate;
		}
	}
}
