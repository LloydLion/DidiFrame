using Microsoft.Extensions.Logging;

namespace CGZBot3.Utils.StateMachine
{
	internal delegate void StateChangedEventHandler<TState>(IStateMachine<TState> stateMahcine, TState oldState) where TState : struct;


	internal interface IStateMachine<TState> : IDisposable where TState : struct
	{
		public event StateChangedEventHandler<TState>? StateChanged;


		public TState? CurrentState { get; }

		public ILogger Logger { get; }


		public void UpdateState();

		public void Start(TState startState);

		public Task AwaitForState(TState? state);
	}
}
