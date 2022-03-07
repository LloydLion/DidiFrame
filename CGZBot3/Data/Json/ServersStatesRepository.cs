using CGZBot3.Utils;

namespace CGZBot3.Data.Json
{
	internal class ServersStatesRepository<TModel> : IServersStatesRepository<TModel> where TModel : class
	{
		private readonly JsonContext context;
		private readonly string key;
		private readonly IModelFactoryProvider provider;
		private readonly ThreadLocker<IServer> locker = new();


		public ServersStatesRepository(JsonContext context, IModelFactoryProvider provider, string key)
		{
			this.context = context;
			this.key = key;
			this.provider = provider;
		}


		public Task PreloadDataAsync()
		{
			return context.LoadAllAsync();
		}

		public ObjectHolder<TModel> GetState(IServer server)
		{
			var lockFree = locker.Lock(server);

			var factory = provider.GetFactory<TModel>();
			var obj = context.Load(server, key, factory);

			return new ObjectHolder<TModel>(obj, (holder) =>
			{
				context.Put(server, key, obj);
				lockFree.Dispose();
			});
		}


		public class Options
		{
			public string BaseDirectory { get; set; } = "";
		}
	}
}
