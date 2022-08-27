using DidiFrame.Data.Model;
using DidiFrame.Utils;

namespace DidiFrame.Data.ContextBased
{
	internal class ContextBasedStatesRepository<TModel> : IServersStatesRepository<TModel> where TModel : class, IDataEntity
	{
		private readonly ThreadLocker<IServer> perServerLock;
		private readonly IDataContext ctx;
		private readonly IModelFactory<TModel> factory;
		private readonly string key;


		public ContextBasedStatesRepository(ThreadLocker<IServer> perServerLock, IDataContext ctx, IModelFactoryProvider provider, string key)
		{
			this.perServerLock = perServerLock;
			this.ctx = ctx;
			factory = provider.GetFactory<TModel>();
			this.key = key;
		}


		public IObjectController<TModel> GetState(IServer server)
		{
			var syncRoot = new ThreadLocker<IServer>.Agent(perServerLock, server);
			return new StateObjectController<TModel>(syncRoot, server, FinalizeObject, GetObject);
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
