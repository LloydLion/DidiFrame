namespace CGZBot3.Data
{
	public interface IServersStatesRepositoryFactory
	{
		public IServersStatesRepository<TModel> Create<TModel>(string key) where TModel : class;

		public Task PreloadDataAsync();
	}
}
