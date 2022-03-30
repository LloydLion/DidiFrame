namespace CGZBot3.Data.Lifetime
{
	public class ServersLifetimesRepository<TLifetime, TBase> : IServersLifetimesRepository<TLifetime, TBase> where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		private readonly ILifetimeFactory<TLifetime, TBase> factory;
		private readonly Dictionary<IServer, Dictionary<TBase, (TLifetime, LifetimeHolder<TLifetime, TBase>.EventFire)>> lifetimes = new();
		private readonly ILifetimeStateUpdater<TBase> updater;


		//Must be not integrated in di container!!
		public ServersLifetimesRepository(ILifetimeFactory<TLifetime, TBase> factory, LifetimeStateUpdater<TBase> updater)
		{
			this.factory = factory;
			this.updater = updater;

			updater.Updated += OnUpdate;
			updater.Finished += OnFinalize;
		}


		public LifetimeHolder<TLifetime, TBase> AddLifetime(TBase baseObject)
		{
			var lt = factory.Create(baseObject);
			lt.Run(updater);
			if (!lifetimes.ContainsKey(baseObject.Server)) lifetimes.Add(baseObject.Server, new());

			var holder = LifetimeHolder<TLifetime, TBase>.Create(lt, out var fire);
			lifetimes[baseObject.Server].Add(baseObject, (lt, fire));
			updater.Update(lt); //Add into state

			return holder;
		}

		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server)
		{
			return lifetimes[server].Values.Select(s => s.Item1).ToArray();
		}

		public TLifetime GetLifetime(TBase baseObject)
		{
			return lifetimes[baseObject.Server][baseObject].Item1;
		}

		private void OnUpdate(ILifetime<TBase> lifetime)
		{
			var baseObj = lifetime.GetBaseClone();
			lifetimes[baseObj.Server][baseObj].Item2.InvokeStateUpdated();
		}

		private void OnFinalize(ILifetime<TBase> lifetime)
		{
			var baseObj = lifetime.GetBaseClone();
			lifetimes[baseObj.Server][baseObj].Item2.InvokeStateUpdated();
		}
	}
}
