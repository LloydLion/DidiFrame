namespace DidiFrame.Data.ContextBased
{
	internal class ContextBasedSettingsRepository<TModel> : IServersSettingsRepository<TModel> where TModel : class
	{
		private readonly IDataContext ctx;
		private readonly string key;


		public ContextBasedSettingsRepository(IDataContext ctx, string key)
		{
			this.ctx = ctx;
			this.key = key;
		}


		public TModel Get(IServer server)
		{
			return ctx.Load<TModel>(server, key);
		}

		public void PostSettings(IServer server, TModel settings)
		{
			ctx.Put(server, key, settings);
		}
	}
}
