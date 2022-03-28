﻿using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Data.Lifetime
{
	internal class ServersLifetimesRepositoryFactory : IServersLifetimesRepositoryFactory
	{
		private readonly IServersStatesRepositoryFactory factory;
		private readonly IServiceProvider provider;
		private readonly List<object> lifetimeRepositories = new();


		public ServersLifetimesRepositoryFactory(IServersStatesRepositoryFactory factory, IServiceProvider provider)
		{
			this.factory = factory;
			this.provider = provider;
		}


		public IServersLifetimesRepository<TLifetime, TBase> Create<TLifetime, TBase>(string stateKey)
			where TLifetime : ILifetime<TBase>
			where TBase : class, ILifetimeBase
		{
			var sod = lifetimeRepositories.SingleOrDefault(s => s is IServersLifetimesRepository<TLifetime, TBase>);
			if (sod is not null) return (IServersLifetimesRepository<TLifetime, TBase>)sod;
			else
			{
				var updater = new LifetimeStateUpdater<TBase>(factory.Create<ICollection<TBase>>(stateKey));
				var ltfactory = provider.GetRequiredService<ILifetimeFactory<TLifetime, TBase>>();
				var repository = new ServersLifetimesRepository<TLifetime, TBase>(ltfactory, updater);
				lifetimeRepositories.Add(repository);
				return repository;
			}
		}
	}
}
