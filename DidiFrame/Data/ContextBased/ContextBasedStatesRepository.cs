using DidiFrame.Utils;

namespace DidiFrame.Data.ContextBased
{
	internal class ContextBasedStatesRepository<TModel> : IServersStatesRepository<TModel> where TModel : class
	{
		private readonly IDataContext ctx;
		private readonly IModelFactory<TModel> factory;
		private readonly string key;


		public ContextBasedStatesRepository(IDataContext ctx, IModelFactoryProvider provider, string key)
		{
			this.ctx = ctx;
			factory = provider.GetFactory<TModel>();
			this.key = key;
		}


		public ServerStateHolder<TModel> GetState(IServer server)
		{
			return new ServerStateHolder<TModel>(GetObject, FinalizeObject, server);
		}

		private TModel GetObject(IServer server)
		{
			return ctx.Load(server, key, factory);
		}

		private void FinalizeObject(IServer server, TModel model)
		{
			ctx.Put(server, key, model);
		}
	}
}
