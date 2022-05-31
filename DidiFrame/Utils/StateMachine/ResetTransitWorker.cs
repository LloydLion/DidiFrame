using System.Diagnostics;

namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Statemachine transit that based on tasks, but always active
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	public class ResetTransitWorker<TState> : IStateTransitWorker<TState> where TState : struct
	{
		private static readonly EventId TaskErrorID = new(12, "TaskError");


		private readonly TState? tstate;
		private readonly Func<CancellationToken, Task> waitTaskFactory;
		private CancellationTokenSource cts = new();
		private Task? currentTask;
		private IStateMachine<TState>? stateMachine;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.StateMachine.ResetTransitWorker`1 using task factory
		/// </summary>
		/// <param name="tstate">Target state of transit (if not null reset transit stays active)</param>
		/// <param name="waitTaskFactory">Factory that will produce tasks for transit</param>
		public ResetTransitWorker(TState? tstate, Func<CancellationToken, Task> waitTaskFactory)
		{
			this.tstate = tstate;
			this.waitTaskFactory = waitTaskFactory;
		}


		/// <inheritdoc/>
		public bool CanActivate(TState state) => true;

		/// <inheritdoc/>
		public bool CanDoTransit() => currentTask?.IsCompleted ?? throw new ImpossibleVariantException();

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public void Activate(IStateMachine<TState> stateMahcine)
		{
			stateMachine = stateMahcine;
			currentTask = waitTaskFactory(cts.Token);
		}

		/// <inheritdoc/>
		public TState? DoTransit()
		{
			return tstate;
		}
	}
}
