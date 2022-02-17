namespace CGZBot3.Utils.StateMachine
{
	internal interface IStateMachineBuilder<TState> where TState : struct
	{
		public void AddStateTransitWorker(IStateTransitWorker<TState> worker);

		public void AddStateChangedHandler(StateChangedEventHandler<TState> handler);

		public IStateMachine<TState> Build();
	}
}