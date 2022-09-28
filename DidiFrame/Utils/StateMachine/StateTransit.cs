namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Represetns statemachine transit
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	/// <param name="Router">Router of transit</param>
	/// <param name="Worker">Worker of transit</param>
	public record struct StateTransit<TState>(IStateTransitRouter<TState> Router, IStateTransitWorker<TState> Worker) where TState : struct;
}
