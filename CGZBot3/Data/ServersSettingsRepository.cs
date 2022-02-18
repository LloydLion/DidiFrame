using CGZBot3.Data.Database;

namespace CGZBot3.Data
{
	internal class ServersSettingsRepository : IServersSettingsRepository
	{
		private readonly SettingsDatabaseContext context;


		public ServersSettingsRepository(SettingsDatabaseContext context)
		{
			this.context = context;
		}


		public Task DeleteServerAsync(IServer server)
		{
			var dbset = context.GetSettings();
			dbset.Remove(dbset.Single(s => s.ServerId == server.Id));

			return context.SaveChangesAsync();
		}

		public async Task<ServerSettings> GetOrCreateAsync(IServer server)
		{
			var dbset = context.GetSettings();

			var value = dbset.SingleOrDefault(s => s.ServerId == server.Id);

			if (value is not null) return value;
			else
			{
				value = new ServerSettings() { ServerId = server.Id };

				context.Add(value);
				await context.SaveChangesAsync();

				return value;
			}
		}

		public Task PostSettingsAsync(IServer server, ServerSettings settings)
		{
			if (server.Id == settings.ServerId)
				throw new ArgumentException("Setting contains invalid ServerId, it must equals server's id", nameof(settings));

			if (context.GetSettings().Any(s => s.ServerId == settings.ServerId))
				context.Update(settings);
			else context.Add(settings);

			return context.SaveChangesAsync();
		}
	}
}
