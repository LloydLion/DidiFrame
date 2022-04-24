namespace DidiFrame.Data.Json
{
	internal class ServersStatesRepositoryFactory : IServersStatesRepositoryFactory
	{
		private readonly JsonContext context;
		private readonly IModelFactoryProvider factoryProvider;


		public ServersStatesRepositoryFactory(IOptions<DataOptions> options, IModelFactoryProvider factoryProvider, ILogger<ServersStatesRepositoryFactory> logger)
		{
			if (options.Value.States is null) throw new ArgumentNullException(nameof(options), "options.Value.States was null, it means that component have disabled and can't be used");
			context = new(options.Value.States.BaseDirectory, logger);
			this.factoryProvider = factoryProvider;
		}


		public IServersStatesRepository<TModel> Create<TModel>(string key) where TModel : class
		{
			return new ServersStatesRepository<TModel>(context, factoryProvider, key);
		}

		public Task PreloadDataAsync()
		{
			return context.LoadAllAsync();
		}
	}
}
