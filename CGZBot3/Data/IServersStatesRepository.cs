namespace CGZBot3.Data
{
	internal interface IServersStatesRepository
	{
		public TModel GetOrCreate<TModel>(IServer server, string key) where TModel : class;

		public void DeleteServer(IServer server, string key);

		public void DeleteServer(IServer server);

		public void Update<TModel>(IServer server, TModel state, string key) where TModel : class;

		public Task PreloadDataAsync();
	}
}
