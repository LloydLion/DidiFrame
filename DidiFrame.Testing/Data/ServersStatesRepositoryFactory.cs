using DidiFrame.Data;

namespace DidiFrame.Testing.Data
{
	/// <summary>
	/// Test IServersStatesRepositoryFactory implementation
	/// </summary>
	public class ServersStatesRepositoryFactory : IServersStatesRepositoryFactory
	{
		private readonly Dictionary<string, ServersStatesRepository> repositories = new();
		private readonly IModelFactoryProvider modelFactoryProvider;


		/// <summary>
		/// Creates new instance of DidiFrame.Testing.Data.ServersStatesRepositoryFactory
		/// </summary>
		/// <param name="modelFactoryProvider"></param>
		public ServersStatesRepositoryFactory(IModelFactoryProvider modelFactoryProvider)
		{
			this.modelFactoryProvider = modelFactoryProvider;
		}


		/// <inheritdoc/>
		public IServersStatesRepository<TModel> Create<TModel>(string key) where TModel : class => CreateTest<TModel>(key);

		/// <summary>
		/// Create new repository
		/// </summary>
		/// <typeparam name="TModel">Type of repository</typeparam>
		/// <param name="key">State key in servers' states</param>
		/// <returns>New repository</returns>
		public ServersStatesRepository<TModel> CreateTest<TModel>(string key) where TModel : class
		{
			if (repositories.ContainsKey(key) == false)
				repositories.Add(key, new ServersStatesRepository<TModel>(modelFactoryProvider.GetFactory<TModel>()));
			return (ServersStatesRepository<TModel>)repositories[key];
		}

		/// <inheritdoc/>
		public Task PreloadDataAsync()
		{
			return Task.CompletedTask;
		}
	}
}
