using Microsoft.Extensions.Logging;

namespace DidiFrame.Utils.StateMachine
{
	public class StateMachine<TState> : IStateMachine<TState> where TState : struct
	{
		private static readonly EventId ActiveWorkersRecalcID = new(32, "ActiveWorkersRecalc");
		private static readonly EventId StateChangedID = new(45, "StateChanged");
		private static readonly EventId StateChangingID = new(42, "StateChanging");
		private static readonly EventId MachineStartupID = new(14, "MachineStartup");
		private static readonly EventId StateChangedHandlerErrorID = new(65, "StateChangedHandlerError");
		private static readonly EventId InternalErrorID = new(65, "InternalError");


		private readonly List<IStateTransitWorker<TState>> workers;
		private readonly List<IStateTransitWorker<TState>> activeWorkers = new();
		private readonly Task observeTask;
		private static readonly ThreadLocker<StateMachine<TState>> locker = new();


		public StateMachine(IReadOnlyList<IStateTransitWorker<TState>> workers, ILogger logger)
		{
			this.workers = workers.ToList();
			Logger = logger;

			observeTask = new Task(() =>
			{
				while (CurrentState != null)
				{
					try { UpdateStateInternal(); }
					catch (Exception ex)
					{ Logger.Log(LogLevel.Warning, InternalErrorID, ex, "State machine internal error"); }

					Thread.Sleep(200);
				}
			});
		}


		public TState? CurrentState { get; private set; }

		public ILogger Logger { get; }



		public event StateChangedEventHandler<TState>? StateChanged;

		
		public StateUpdateResult<TState> UpdateState()
		{
			using(locker.Lock(this)) return UpdateStateNoLock();
		}

		private StateUpdateResult<TState> UpdateStateNoLock()
		{
			var fod = activeWorkers.FirstOrDefault(s => s.CanDoTransit());
			if (fod == null) return new StateUpdateResult<TState>(false, null);

			Logger.Log(LogLevel.Trace, StateChangingID, "StateTransitWorker which can does transit found. Current state - {CurrentState}", CurrentState);

			var oldState = CurrentState ?? throw new ImpossibleVariantException();

			CurrentState = fod.DoTransit();

			try { StateChanged?.Invoke(this, oldState); }
			catch (Exception ex) { Logger.Log(LogLevel.Warning, StateChangedHandlerErrorID, ex, "Some StateChanged event handler executed with error"); }

			if (CurrentState is not null) UpdateActiveWorkers();
			else activeWorkers.Clear();

			Logger.Log(LogLevel.Trace, StateChangedID, "State changed from {OldState} to {State} by {WorkerType} #{Index}",
				oldState, CurrentState?.ToString() ?? "[NULL]", fod.GetType().ToString(), workers.IndexOf(fod));

			return new StateUpdateResult<TState>(true, CurrentState);
		}
		
		private void UpdateStateInternal()
		{
			using (locker.Lock(this))
			{
				var fod = activeWorkers.FirstOrDefault(s => s.CanDoTransit());
				if (fod == null) return;

				Logger.Log(LogLevel.Trace, StateChangingID, "StateTransitWorker which can does transit found. Current state - {CurrentState}", CurrentState);

				var oldState = CurrentState ?? throw new ImpossibleVariantException();

				CurrentState = fod.DoTransit();

				try { StateChanged?.Invoke(this, oldState); }
				catch (Exception ex) { Logger.Log(LogLevel.Warning, StateChangedHandlerErrorID, ex, "Some StateChanged event handler executed with error"); }

				if (CurrentState is not null) UpdateActiveWorkers();
				else activeWorkers.Clear();

				Logger.Log(LogLevel.Trace, StateChangedID, "State changed from {OldState} to {State} by {WorkerType} #{Index}",
					oldState, CurrentState?.ToString() ?? "[NULL]", fod.GetType().ToString(), workers.IndexOf(fod));
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
			GC.SuppressFinalize(this);
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
				while (!CurrentState.Equals(state)) Thread.Sleep(100);
			});
		}

		public FreezeModel<TState> Freeze()
		{
			var lockFree = locker.Lock(this);

			return new FreezeModel<TState>(() =>
			{
				//ORDER is important!
				var result = UpdateStateNoLock();
				lockFree.Dispose();
				return result;
			});
		}
	}
}
