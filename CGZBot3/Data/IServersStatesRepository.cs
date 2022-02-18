namespace CGZBot3.Data
{
	internal interface IServersStatesRepository
	{
		public Task<ServerState> GetOrCreateAsync(IServer server);

		public Task DeleteServerAsync(IServer server);

		public Task UpdateAsync(IServer server, ServerState state);
	}
}
