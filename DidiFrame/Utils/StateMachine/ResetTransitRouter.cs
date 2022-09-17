namespace DidiFrame.Utils.StateMachine
{
	public class ResetTransitRouter<TState> : IStateTransitRouter<TState> where TState : struct
	{
		public bool CanActivate(TState state) => true;

		public TState? SwitchState(TState oldState) => null;
	}
}
