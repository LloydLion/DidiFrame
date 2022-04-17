using CGZBot3.Data;
using CGZBot3.Data.Lifetime;
using System.Collections.Generic;
using System.Linq;

namespace TestProject.Environment.Data
{
	internal class ServersLifetimesRepositoryFactory : IServersLifetimesRepositoryFactory
	{
		private readonly Dictionary<string, List<ServersLifetimesRepository>> data = new();
		private readonly IServersStatesRepositoryFactory states;


		public ServersLifetimesRepositoryFactory(IServersStatesRepositoryFactory states)
		{
			this.states = states;
		}


		public IServersLifetimesRepository<TLifetime, TBase> Create<TLifetime, TBase>(string stateKey)
			where TLifetime : ILifetime<TBase>
			where TBase : class, ILifetimeBase
		{
			return (ServersLifetimesRepository<TLifetime, TBase>)data[stateKey].Single(s => s is ServersLifetimesRepository<TLifetime, TBase>);
		}

		public void AddRepository<TLifetime, TBase>(ServersLifetimesRepository<TLifetime, TBase> repository, ILifetimeFactory<TLifetime, TBase> factory, string stateKey)
			where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
		{
			repository.Init(factory, new LifetimeStateUpdater<TBase>(states.Create<ICollection<TBase>>(stateKey)));
			if (data.ContainsKey(stateKey) == false) data.Add(stateKey, new());
			data[stateKey].Add(repository);
		}
	}
}
