namespace DidiFrame.Clients.DSharp.Mutations
{
	public class MutationResult<TState> where TState : struct
	{
		public MutationResult(TState oldState, TState newState)
		{
			OldState = oldState;
			NewState = newState;
		}


		public bool IsStateChanged => Equals(OldState, NewState) == false;

		public TState OldState { get; }

		public TState NewState { get; }
	}
}