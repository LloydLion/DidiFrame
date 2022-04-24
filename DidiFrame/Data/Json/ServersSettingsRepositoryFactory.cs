namespace DidiFrame.Data.Json
{
	internal class ServersSettingsRepositoryFactory : IServersSettingsRepositoryFactory
	{
		private readonly JsonContext context;


		public ServersSettingsRepositoryFactory(IOptions<DataOptions> options, ILogger<ServersSettingsRepositoryFactory> logger)
		{
			if (options.Value.Settings is null) throw new ArgumentNullException(nameof(options), "options.Value.Settings was null, it means that component have disabled and can't be used");
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
