using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Data.Lifetime
{
	internal class ServersLifetimesRepositoryFactory : IServersLifetimesRepositoryFactory
	{
		private readonly IServersStatesRepositoryFactory factory;
		private readonly IServiceProvider provider;


		public ServersLifetimesRepositoryFactory(IServersStatesRepositoryFactory factory, IServiceProvider provider)
		{
			this.factory = factory;
			this.provider = provider;
		}


		public IServersLifetimesRepository<TLifetime, TBase> Create<TLifetime, TBase>(string stateKey)
			where TLifetime : ILifetime<TBase>
			where TBase : class, ILifetimeBase
		{
			var updater = new LifetimeStateUpdater<TBase>(factory.Create<ICollection<TBase>>(stateKey));
			var ltfactory = provider.GetRequiredService<ILifetimeFactory<TLifetime, TBase>>();
			return new ServersLifetimesRepository<TLifetime, TBase>(ltfactory, updater);
		}
	}
}
