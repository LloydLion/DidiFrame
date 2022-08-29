using DidiFrame.Data;

namespace DidiFrame.Testing.Data
{
	public class ServersStatesRepositoryFactory : IServersStatesRepositoryFactory
	{
		private readonly Dictionary<string, ServersStatesRepository> repositories = new();
		private readonly IModelFactoryProvider modelFactoryProvider;


		public ServersStatesRepositoryFactory(IModelFactoryProvider modelFactoryProvider)
		{
			this.modelFactoryProvider = modelFactoryProvider;
		}


		public IServersStatesRepository<TModel> Create<TModel>(string key) where TModel : class => CreateTest<TModel>(key);

		public ServersStatesRepository<TModel> CreateTest<TModel>(string key) where TModel : class
		{
			if (repositories.ContainsKey(key) == false)
				repositories.Add(key, new ServersStatesRepository<TModel>(modelFactoryProvider.GetFactory<TModel>()));
			return (ServersStatesRepository<TModel>)repositories[key];
		}

		public Task PreloadDataAsync()
		{
			return Task.CompletedTask;
		}
	}
}
