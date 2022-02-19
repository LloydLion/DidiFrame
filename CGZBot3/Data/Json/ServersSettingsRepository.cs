namespace CGZBot3.Data.Json
{
	internal class ServersSettingsRepository : IServersSettingsRepository
	{
		private readonly JsonContext context;


		public ServersSettingsRepository(IOptions<Options> options)
		{
			context = new JsonContext(options.Value.BaseDirectory);
		}


		public void DeleteServer(IServer server, string key)
		{
			context.Delete(server, key);
		}

		public void DeleteServer(IServer server)
		{
			context.DeleteAll(server);
		}

		public TModel GetOrCreate<TModel>(IServer server, string key) where TModel : class
		{
			return context.Load<TModel>(server, key);
		}

		public void PostSettings<TModel>(IServer server, TModel settings, string key) where TModel : class
		{
			context.Put(server, key, settings);
		}


		public class Options
		{
			public string BaseDirectory { get; set; } = "";
		}
	}
}
