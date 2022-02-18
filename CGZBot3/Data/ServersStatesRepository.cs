using CGZBot3.Data.Database;

namespace CGZBot3.Data
{
	internal class ServersStatesRepository : IServersStatesRepository
	{
		public readonly StateDatabaseContext context;


		public ServersStatesRepository(StateDatabaseContext context)
		{
			this.context = context;
		}


		public Task DeleteServerAsync(IServer server)
		{
			var dbset = context.GetGlobalState();
			dbset.Remove(dbset.Single(s => s.ServerId == server.Id));

			return context.SaveChangesAsync();
		}

		public async Task<ServerState> GetOrCreateAsync(IServer server)
		{
			var dbset = context.GetGlobalState();

			var value = dbset.SingleOrDefault(s => s.ServerId == server.Id);

			if (value is not null) return value;
			else
			{
				value = new ServerState() { ServerId = server.Id };

				context.Add(value);
				await context.SaveChangesAsync();

				return value;
			}
		}

		public Task UpdateAsync(IServer server, ServerState state)
		{
			if (server.Id == state.ServerId)
				throw new ArgumentException("State contains invalid ServerId, it must equals server's id", nameof(state));

			context.Update(state);

			return context.SaveChangesAsync();
		}
	}
}
