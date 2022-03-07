namespace CGZBot3.Data
{
	internal interface IServersSettingsRepository
	{
		public TModel Get<TModel>(IServer server, string key) where TModel : class;

		public void DeleteServer(IServer server, string key);

		public void DeleteServer(IServer server);

		public void PostSettings<TModel>(IServer server, TModel settings, string key) where TModel : class;

		public Task PreloadDataAsync();
	}
}
