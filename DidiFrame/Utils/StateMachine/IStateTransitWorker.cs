namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Represents a statemachine inter-state transit
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	public interface IStateTransitWorker<TState> where TState : struct
	{
		/// <summary>
		/// Activates transit, statemachine calls it when CanActivate(TState) becomes true
		/// </summary>
		/// <param name="stateMahcine">Statemachine that called method</param>
		public void Activate(IStateMachine<TState> stateMahcine);

		/// <summary>
		/// Checks if can activate transit on given state
		/// </summary>
		/// <param name="state">Checking state of statemachine</param>
		/// <returns>If can activate transit, if can it will activated by statamachine using Activate() method</returns>
		public bool CanActivate(TState state);

		/// <summary>
		/// Checks if transit ready to switch state of statemahcine
		/// </summary>
		/// <returns>If can switch, if can statemachine will call DoTransit() method</returns>
		public bool CanDoTransit();

		/// <summary>
		/// Return target state of transit or null to finalize statemachine
		/// </summary>
		/// <returns>Target state or null</returns>
		public TState? DoTransit();

		/// <summary>
		/// Disactivates transit, statemachine calls it when some (this or other) transit switches statemachine state and transit is active
		/// </summary>
		public void Disactivate();
	}
}
