﻿namespace CGZBot3.Data.Lifetime
{
	public class ServersLifetimesRepository<TLifetime, TBase> : IServersLifetimesRepository<TLifetime, TBase> where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		private readonly ILifetimeFactory<TLifetime, TBase> factory;
		private readonly Dictionary<IServer, Dictionary<TBase, TLifetime>> lifetimes = new();
		private readonly ILifetimeStateUpdater<TBase> updater;


		//Must be not integrated in di container!!
		public ServersLifetimesRepository(ILifetimeFactory<TLifetime, TBase> factory, ILifetimeStateUpdater<TBase> updater)
		{
			this.factory = factory;
			this.updater = updater;
		}


		public TLifetime AddLifetime(TBase baseObject)
		{
			var lt = factory.Create(baseObject);
			lt.Run(updater);
			if (!lifetimes.ContainsKey(baseObject.Server)) lifetimes.Add(baseObject.Server, new());
			lifetimes[baseObject.Server].Add(baseObject, lt);
			updater.Update(lt); //Add into state
			return lt;
		}

		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server)
		{
			return lifetimes[server].Values;
		}

		public TLifetime GetLifetime(TBase baseObject)
		{
			return lifetimes[baseObject.Server][baseObject];
		}
	}
}
