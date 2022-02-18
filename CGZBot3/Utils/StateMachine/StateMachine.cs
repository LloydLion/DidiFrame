﻿using Microsoft.Extensions.Logging;

namespace CGZBot3.Utils.StateMachine
{
	internal class StateMachine<TState> : IStateMachine<TState> where TState : struct
	{
		private static readonly EventId ActiveWorkersRecalcID = new(32, "ActiveWorkersRecalc");
		private static readonly EventId StateChangedID = new(45, "StateChanged");
		private static readonly EventId StateChangingID = new(42, "StateChanging");
		private static readonly EventId MachineStartupID = new(14, "MachineStartup");


		private readonly List<IStateTransitWorker<TState>> workers;
		private readonly List<IStateTransitWorker<TState>> activeWorkers = new();
		private readonly Task observeTask;


		public StateMachine(IReadOnlyList<IStateTransitWorker<TState>> workers, ILogger logger)
		{
			this.workers = workers.ToList();
			Logger = logger;

			observeTask = new Task(() =>
			{
				while (CurrentState != null)
				{
					UpdateState();
					Thread.Sleep(100);
				}
			});
		}


		public TState? CurrentState { get; private set; }

		public ILogger Logger { get; }



		public event StateChangedEventHandler<TState>? StateChanged;

		
		public void UpdateState()
		{
			lock(this)
			{
				using (Logger.BeginScope("State update"))
				{
					var fod = activeWorkers.FirstOrDefault(s => s.CanDoTransit());
					if (fod == null) return;

					Logger.Log(LogLevel.Trace, StateChangingID, "StateTransitWorker which can does transit found. Current state - {CurrentState}", CurrentState);

					var oldState = CurrentState ?? throw new ImpossibleVariantException();

					using (Logger.BeginScope("{WorkerType} #{Index}", fod.GetType().ToString(), workers.IndexOf(fod)))
						CurrentState = fod.DoTransit();

					StateChanged?.Invoke(this, oldState);

					UpdateActiveWorkers();

					Logger.Log(LogLevel.Trace, StateChangedID, "State changed from {OldState} to {State} by {WorkerType} #{Index}",
						oldState, CurrentState, fod.GetType().ToString(), workers.IndexOf(fod));
				}
			}
		}

		public void Start(TState startState)
		{
			CurrentState = startState;
			UpdateActiveWorkers();
			observeTask.Start();

			Logger.Log(LogLevel.Debug, MachineStartupID, "Machine has started");
		}

		public void Dispose()
		{
			if (observeTask.IsCompleted == false) throw new InvalidOperationException("Can't dispose working state machine");
		}

		private void UpdateActiveWorkers()
		{
			foreach (var worker in activeWorkers)
				using (Logger.BeginScope("{WorkerType} #{Index}", worker.GetType().ToString(), workers.IndexOf(worker)))
					worker.Disactivate();

			activeWorkers.Clear();
			activeWorkers.AddRange(workers.Where(s => s.CanActivate(CurrentState ?? throw new ImpossibleVariantException())));

			foreach (var worker in activeWorkers)
				using (Logger.BeginScope("{WorkerType} #{Index}", worker.GetType().ToString(), workers.IndexOf(worker)))
					worker.Activate(this);

			Logger.Log(LogLevel.Trace, ActiveWorkersRecalcID, "Machine's old workers disactivated and new workers for {State} state activated", CurrentState);
		}

		public Task AwaitForState(TState? state)
		{
			return Task.Run(() =>
			{
				while (!CurrentState.Equals(state))
					lock(this)
						Thread.Sleep(100);
			});
		}
	}
}