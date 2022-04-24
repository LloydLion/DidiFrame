namespace DidiFrame.Utils.StateMachine
{
	public delegate void StateChangedEventHandler<TState>(IStateMachine<TState> stateMahcine, TState oldState) where TState : struct;


	public interface IStateMachine<TState> : IDisposable where TState : struct
	{
		public event StateChangedEventHandler<TState>? StateChanged;


		public TState? CurrentState { get; }

		public ILogger Logger { get; }


		public StateUpdateResult<TState> UpdateState();

		public void Start(TState startState);

		public Task AwaitForState(TState? state);

		public FreezeModel<TState> Freeze();
	}
}
