using CGZBot3.Data.Lifetime;
using System;
using System.Collections.Generic;

namespace TestProject.Environment.Data
{
	internal class ServersLifetimesRepository<TLifetime, TBase> : ServersLifetimesRepository, IServersLifetimesRepository<TLifetime, TBase>
		where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		private readonly Dictionary<IServer, Dictionary<TBase, TLifetime>> lifetimes = new();
		private ILifetimeFactory<TLifetime, TBase>? factory;
		private ILifetimeStateUpdater<TBase>? updater;


		public TLifetime AddLifetime(TBase baseObject)
		{
			if (factory is null || updater is null) throw new NullReferenceException();

			var lt = factory.Create(baseObject);
			lt.Run(updater);
			if (!lifetimes.ContainsKey(baseObject.Server)) lifetimes.Add(baseObject.Server, new());
			lifetimes[baseObject.Server].Add(baseObject, lt);
			updater.Update(lt); //Add into state
			updater.Finished += handler;
			return lt;

			void handler(ILifetime<TBase> lifetime)
			{
				if (lifetime.Equals(lt))
				{
					lifetimes[baseObject.Server].Remove(baseObject);
					updater.Finished -= handler;
				}
			}
		}

		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server)
		{
			if (lifetimes.ContainsKey(server) == false) lifetimes.Add(server, new());
			return lifetimes[server].Values;
		}

		public TLifetime GetLifetime(TBase baseObject)
		{
			return lifetimes[baseObject.Server][baseObject];
		}

		public void Init(ILifetimeFactory<TLifetime, TBase> factory, ILifetimeStateUpdater<TBase> updater)
		{
			this.factory = factory;
			this.updater = updater;
		}
	}

	internal abstract class ServersLifetimesRepository { }
}
