using DidiFrame.Utils;

namespace DidiFrame.Data.ContextBased
{
	internal class ContextBasedStatesRepository<TModel> : IServersStatesRepository<TModel> where TModel : class
	{
		private readonly IDataContext ctx;
		private readonly IModelFactoryProvider provider;
		private readonly string key;
		private readonly ThreadLocker<IServer> locker = new();


		public ContextBasedStatesRepository(IDataContext ctx, IModelFactoryProvider provider, string key)
		{
			this.ctx = ctx;
			this.provider = provider;
			this.key = key;
		}


		public ObjectHolder<TModel> GetState(IServer server)
		{
			var lockFree = locker.Lock(server);

			var factory = provider.GetFactory<TModel>();
			var obj = ctx.Load(server, key, factory);

			return new ObjectHolder<TModel>(obj, (holder) =>
			{
				ctx.Put(server, key, obj);
				lockFree.Dispose();
			});
		}
	}
}
