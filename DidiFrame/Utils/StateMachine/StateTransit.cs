namespace DidiFrame.Utils.StateMachine
{
	public record struct StateTransit<TState>(IStateTransitRouter<TState> Router, IStateTransitWorker<TState> Worker) where TState : struct;
}
