namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Represents a statemachine inter-state transit router that determines where need to do transit
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	public interface IStateTransitRouter<TState> where TState : struct
	{
		/// <summary>
		/// Checks if can activate transit on given state
		/// </summary>
		/// <param name="state">Checking state of statemachine</param>
		/// <returns>If can activate transit, if can it will activated by statamachine using Activate() method</returns>
		public bool CanActivate(TState state);

		/// <summary>
		/// Return target state of transit or null to finalize statemachine
		/// </summary>
		/// <returns>Target state or null</returns>
		public TState? SwitchState(TState oldState);
	}
}
