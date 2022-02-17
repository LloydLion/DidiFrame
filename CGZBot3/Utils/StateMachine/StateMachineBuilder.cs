namespace CGZBot3.Utils.StateMachine
{
	internal class StateMachineBuilder<TState> : IStateMachineBuilder<TState> where TState : struct
	{
		private readonly ILogger logger;
		private readonly StateMachineProfile<TState>.Builder profile = new();


		public StateMachineBuilder(ILogger logger)
		{
			this.logger = logger;
		}


		public void AddStateTransitWorker(IStateTransitWorker<TState> worker) => profile.AddStateTransitWorker(worker);

		public void AddStateChangedHandler(StateChangedEventHandler<TState> handler) =>	profile.AddStateChangedHandler(handler);

		public IStateMachine<TState> Build()
		{
			var pf = profile.Build();

			var sm = new StateMachine<TState>(pf.StateWorkers, logger);
			foreach (var handler in pf.StateChangedHandlers) sm.StateChanged += handler;

			return sm;
		}
	}
}
