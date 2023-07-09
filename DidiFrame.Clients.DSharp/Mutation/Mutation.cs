namespace DidiFrame.Clients.DSharp.Mutations
{
	public delegate TState Mutation<TState>(TState state) where TState : struct;
}