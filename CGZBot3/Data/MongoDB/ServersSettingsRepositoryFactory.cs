namespace CGZBot3.Data.MongoDB
{
	internal class ServersSettingsRepositoryFactory : IServersSettingsRepositoryFactory
	{
		private readonly MongoDBContext context;


		public ServersSettingsRepositoryFactory(IOptions<DataOptions> options, ILogger<ServersSettingsRepositoryFactory> logger, IClient client)
		{
			if (options.Value.Settings is null) throw new ArgumentNullException(nameof(options), "options.Value.Settings was null, it means that component have disabled and can't be used");
			context = new(logger, client, options.Value.Settings.ConnectionString, options.Value.Settings.DatabaseName);
		}


		public IServersSettingsRepository<TModel> Create<TModel>(string key) where TModel : class
		{
			return new ServersSettingsRepository<TModel>(context, key);
		}

		public Task PreloadDataAsync()
		{
			return context.LoadAllAsync();
		}
	}
}
