namespace CGZBot3.Data
{
	public interface IServersSettingsRepositoryFactory
	{
		public IServersSettingsRepository<TModel> Create<TModel>(string key) where TModel : class;

		public Task PreloadDataAsync();
	}
}
