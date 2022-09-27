using System.Diagnostics;
using static DidiFrame.Utils.StateMachine.TaskTransitWorkerStatic;

namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Statemachine transit that based on tasks
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	public sealed class TaskTransitWorker<TState> : IStateTransitWorker<TState> where TState : struct
	{
		private readonly Func<CancellationToken, Task> taskFactory;
		private CancellationTokenSource source = new();
		private Task? currentTask;
		private IStateMachine<TState>? stateMachine;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.StateMachine.TaskTransitWorker`1 using task factory
		/// </summary>
		/// <param name="taskFactory">Factory that will produce tasks for transit</param>
		public TaskTransitWorker(Func<CancellationToken, Task> taskFactory)
		{
			this.taskFactory = taskFactory;
		}


		/// <inheritdoc/>
		public void Activate(IStateMachine<TState> stateMachine)
		{
			currentTask = taskFactory(source.Token);
			this.stateMachine = stateMachine;
		}

		/// <inheritdoc/>
		public bool CanDoTransit() => currentTask?.IsCompleted ?? throw new ImpossibleVariantException();

		/// <inheritdoc/>
		public void Disactivate()
		{
			if (currentTask is null) return;

			source.Cancel();

			var sw = new Stopwatch();
			sw.Start();
				
			while(!currentTask.IsCompleted && sw.ElapsedMilliseconds <= 3500)
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
			source = new();
		}

		/// <inheritdoc/>
		public void DoTransit()
		{
			currentTask = null;
		}
	}

	internal static class TaskTransitWorkerStatic
	{
		public static readonly EventId TaskErrorID = new(12, "TaskError");
	}
}
