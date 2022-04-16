namespace CGZBot3.Utils.StateMachine
{
	public record StateUpdateResult<TState>(bool HasStateUpdated, TState? NewState) where TState : struct;
}
