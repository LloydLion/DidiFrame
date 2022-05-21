using System.Diagnostics;

namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Statemachine transit that based on tasks
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	public class TaskTransitWorker<TState> : AbstractTransitWorker<TState> where TState : struct
	{
		private static readonly EventId TaskErrorID = new(12, "TaskError");


		private readonly Func<CancellationToken, Task> taskFactory;
		private CancellationTokenSource source = new();
		private Task? currentTask;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.StateMachine.TaskTransitWorker`1 using task factory
		/// </summary>
		/// <param name="activation">Activation state (from)</param>
		/// <param name="destination">Destonation state (to)</param>
		/// <param name="taskFactory">Factory that will produce tasks for transit</param>
		public TaskTransitWorker(TState activation, TState? destination, Func<CancellationToken, Task> taskFactory)
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
			source = new();
		}
	}
}
