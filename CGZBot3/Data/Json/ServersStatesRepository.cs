namespace CGZBot3.Data.Json
{
	internal class ServersStatesRepository : IServersStatesRepository
	{
		public readonly JsonContext context;


		public ServersStatesRepository(IOptions<Options> options)
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

		public void Update<TModel>(IServer server, TModel state, string key) where TModel : class
		{
			context.Put(server, key, state);
		}


		public class Options
		{
			public string BaseDirectory { get; set; } = "";
		}
	}
}
