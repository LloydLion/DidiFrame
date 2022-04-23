namespace CGZBot3.Data.MongoDB
{
	internal class ServersStatesRepositoryFactory : IServersStatesRepositoryFactory
	{
		private readonly MongoDBContext context;
		private readonly IModelFactoryProvider factoryProvider;


		public ServersStatesRepositoryFactory(IOptions<DataOptions> options, IModelFactoryProvider factoryProvider, ILogger<ServersStatesRepositoryFactory> logger, IClient client)
		{
			if (options.Value.States is null) throw new ArgumentNullException(nameof(options), "options.Value.States was null, it means that component have disabled and can't be used");
			context = new(logger, client, options.Value.States.ConnectionString, options.Value.States.DatabaseName);
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
