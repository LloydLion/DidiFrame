namespace CGZBot3.Data.Json
{
	internal class ServersSettingsRepositoryFactory : IServersSettingsRepositoryFactory
	{
		private readonly JsonContext context;


		public ServersSettingsRepositoryFactory(IOptions<DataOptions> options, ILogger<ServersSettingsRepositoryFactory> logger)
		{
			context = new(options.Value.Settings.BaseDirectory, logger);
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
