namespace DidiFrame.Data.MongoDB
{
	internal class ServersSettingsRepository<TModel> : IServersSettingsRepository<TModel> where TModel : class
	{
		private readonly MongoDBContext context;
		private readonly string key;


		public ServersSettingsRepository(MongoDBContext context, string key)
		{
			this.context = context;
			this.key = key;
		}


		public TModel Get(IServer server)
		{
			return context.Load<TModel>(server, key);
		}

		public void PostSettings(IServer server, TModel settings)
		{
			context.Put(server, key, settings);
		}
	}
}
