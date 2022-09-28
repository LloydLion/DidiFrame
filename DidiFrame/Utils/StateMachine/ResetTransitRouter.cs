namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Transit worker that always can be activated and switches state only to null
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	public class ResetTransitRouter<TState> : IStateTransitRouter<TState> where TState : struct
	{

		/// <inheritdoc/>
		public bool CanActivate(TState state) => true;


		/// <inheritdoc/>
		public TState? SwitchState(TState oldState) => null;
	}
}
