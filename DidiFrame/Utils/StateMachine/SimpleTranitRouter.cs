namespace DidiFrame.Utils.StateMachine
{
	public class SimpleTranitRouter<TState> : IStateTransitRouter<TState> where TState : struct
	{
		private readonly object startState; //Prevent pack process
		private readonly TState? destonation;


		public SimpleTranitRouter(TState startState, TState? destonation)
		{
			this.startState = startState;
			this.destonation = destonation;
		}


		public bool CanActivate(TState state) => state.Equals(startState);

		public TState? SwitchState(TState oldState) => destonation;
	}
}
