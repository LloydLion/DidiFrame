using static DidiFrame.Utils.StateMachine.StateMachineStatic;

namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Simple implementation of DidiFrame.Utils.StateMachine.IStateMachine`1
	/// </summary>
	/// <typeparam name="TState">Struct that represents state of statemahcine</typeparam>
	public sealed class StateMachine<TState> : IStateMachine<TState> where TState : struct
	{
		private readonly StateTransit<TState>[] transits;
		private readonly List<StateTransit<TState>> activeWorkers = new();
		private static readonly ThreadLocker<StateMachine<TState>> locker = new();


		/// <inheritdoc/>
		public StateMachine(IReadOnlyCollection<StateTransit<TState>> workers, ILogger logger)
		{
			this.transits = workers.ToArray();
			Logger = logger;
		}


		/// <inheritdoc/>
		public TState? CurrentState { get; private set; }

		/// <inheritdoc/>
		public ILogger Logger { get; }


		/// <inheritdoc/>
		public event StateChangedEventHandler<TState>? StateChanged;


		/// <inheritdoc/>
		public StateUpdateResult<TState>? UpdateState()
		{
			using(locker.Lock(this)) return UpdateStateNoLock();
		}

		private StateUpdateResult<TState>? UpdateStateNoLock()
		{
			var targetWorker = activeWorkers.FirstOrDefault(s => s.Worker.CanDoTransit());
			if (targetWorker == default) return null;

			Logger.Log(LogLevel.Trace, StateChangingID, "StateTransitWorker which can does transit found. Current state - {CurrentState}", CurrentState);

			var oldState = CurrentState ?? throw new ImpossibleVariantException();

			CurrentState = targetWorker.Router.SwitchState(oldState);
			targetWorker.Worker.DoTransit();

			try { StateChanged?.Invoke(this, oldState); }
			catch (Exception ex) { Logger.Log(LogLevel.Warning, StateChangedHandlerErrorID, ex, "Some StateChanged event handler executed with error"); }

			if (CurrentState is not null) UpdateActiveWorkers();
			else activeWorkers.Clear();

			Logger.Log(LogLevel.Trace, StateChangedID, "State changed from {OldState} to {State} by {WorkerType} #{Index}",
				oldState, CurrentState?.ToString() ?? "[NULL]", targetWorker.GetType().ToString(), Array.FindIndex(transits, s => Equals(s, targetWorker)));

			return new StateUpdateResult<TState>(CurrentState);
		}

		/// <inheritdoc/>
		public void Start(TState startState)
		{
			CurrentState = startState;
			UpdateActiveWorkers();

			Logger.Log(LogLevel.Debug, MachineStartupID, "State machine has started");
		}

		private void UpdateActiveWorkers()
		{
			foreach (var worker in activeWorkers)
				using (Logger.BeginScope("{WorkerType} #{Index}", worker.GetType().ToString(), Array.FindIndex(transits, s => Equals(s, worker))))
					worker.Worker.Disactivate();

			activeWorkers.Clear();
			activeWorkers.AddRange(transits.Where(s => s.Router.CanActivate(CurrentState ?? throw new ImpossibleVariantException())));

			foreach (var worker in activeWorkers)
				using (Logger.BeginScope("{WorkerType} #{Index}", worker.GetType().ToString(), Array.FindIndex(transits, s => Equals(s, worker))))
					worker.Worker.Activate(this);

			Logger.Log(LogLevel.Trace, ActiveWorkersRecalcID, "Machine's old workers disactivated and new workers for {State} state activated", CurrentState);
		}

		/// <inheritdoc/>
		public Task AwaitForState(TState? state)
		{
			var tcs = new TaskCompletionSource();

			StateChanged += handler;

			return tcs.Task;



			void handler(IStateMachine<TState> _, TState newState)
			{
				if (Equals(newState, state))
				{
					StateChanged -= handler;
					tcs.SetResult();
				}
			}
		}


	}

	internal static class StateMachineStatic
	{
		public static readonly EventId ActiveWorkersRecalcID = new(32, "ActiveWorkersRecalc");
		public static readonly EventId StateChangedID = new(45, "StateChanged");
		public static readonly EventId StateChangingID = new(42, "StateChanging");
		public static readonly EventId MachineStartupID = new(14, "MachineStartup");
		public static readonly EventId StateChangedHandlerErrorID = new(65, "StateChangedHandlerError");
		public static readonly EventId InternalErrorID = new(66, "InternalError");
	}
}
