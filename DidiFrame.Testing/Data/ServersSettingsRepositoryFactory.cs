using DidiFrame.Data;

namespace DidiFrame.Testing.Data
{
	/// <summary>
	/// Test IServersSettingsRepositoryFactory implementation
	/// </summary>
	public class ServersSettingsRepositoryFactory : IServersSettingsRepositoryFactory
	{
		private readonly Dictionary<string, ServersSettingsRepository> repositories = new();


		/// <inheritdoc/>
		public IServersSettingsRepository<TModel> Create<TModel>(string key) where TModel : class => CreateTest<TModel>(key);

		/// <summary>
		/// Creates new repository
		/// </summary>
		/// <typeparam name="TModel">Type of repository</typeparam>
		/// <param name="key">State key in servers' settings</param>
		/// <returns>New repository</returns>
		public ServersSettingsRepository<TModel> CreateTest<TModel>(string key) where TModel : class
		{
			if (repositories.ContainsKey(key) == false)
				repositories.Add(key, new ServersSettingsRepository<TModel>());
			return (ServersSettingsRepository<TModel>)repositories[key];
		}

		/// <inheritdoc/>
		public Task PreloadDataAsync()
		{
			return Task.CompletedTask;
		}
	}
}
