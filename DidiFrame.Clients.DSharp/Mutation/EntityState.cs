namespace DidiFrame.Clients.DSharp.Mutations
{
	public class EntityState<TState> where TState : struct
	{
		private TState internalState;


		public EntityState(TState initialState)
		{
			internalState = initialState;
		}


		public MutationResult<TState> Mutate(Mutation<TState> mutation)
		{
			var oldState = internalState;
			var newState = mutation(internalState);
			internalState = newState;

			return new MutationResult<TState>(oldState, newState);
		}

		public TState AccessState()
		{
			return internalState;
		}
	}
}
