using DidiFrame.Data;

namespace DidiFrame.Testing.Data
{
	public class ServersSettingsRepositoryFactory : IServersSettingsRepositoryFactory
	{
		private readonly Dictionary<string, ServersSettingsRepository> repositories = new();


		public IServersSettingsRepository<TModel> Create<TModel>(string key) where TModel : class => CreateTest<TModel>(key);

		public ServersSettingsRepository<TModel> CreateTest<TModel>(string key) where TModel : class
		{
			if (repositories.ContainsKey(key) == false)
				repositories.Add(key, new ServersSettingsRepository<TModel>());
			return (ServersSettingsRepository<TModel>)repositories[key];
		}

		public Task PreloadDataAsync()
		{
			return Task.CompletedTask;
		}
	}
}
