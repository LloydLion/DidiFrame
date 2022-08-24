namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Represents result of state update operation of statemachine
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	/// <param name="HasStateUpdated">If machine has switched state after operation</param>
	/// <param name="NewState">If machine hasn't switched state null else new state or null if operation finalizer machine</param>
	public record struct StateUpdateResult<TState>(TState? NewState) where TState : struct;
}
