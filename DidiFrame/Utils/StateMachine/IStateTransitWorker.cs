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
		/// <param name="stateMachine">Statemachine that called method</param>
		public void Activate(IStateMachine<TState> stateMachine);

		/// <summary>
		/// Checks if transit ready to switch state of statemahcine
		/// </summary>
		/// <returns>If can switch, if can statemachine will call DoTransit() method</returns>
		public bool CanDoTransit();

		/// <summary>
		/// Checks if transit ready to switch state of statemahcine
		/// </summary>
		/// <returns>If can switch, if can statemachine will call DoTransit() method</returns>
		public void DoTransit();

		/// <summary>
		/// Disactivates transit, statemachine calls it when some (this or other) transit switches statemachine state and transit is active
		/// </summary>
		public void Disactivate();
	}
}
