namespace CGZBot3.Data.Lifetime
{
	public class LifetimeRegistry<TLifetime, TBase> : ILifetimesRegistry where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		private readonly IServerLifetimesRepository<TLifetime, TBase> repository;
		private readonly IServersStatesRepository<ICollection<TBase>> baseRepository;


		//Must be created by custom factory in di container
		public LifetimeRegistry(IServerLifetimesRepository<TLifetime, TBase> repository, IServersStatesRepository<ICollection<TBase>> baseRepository)
		{
			this.repository = repository;
			this.baseRepository = baseRepository;
		}


		public void LoadAndRunAll(IServer server)
		{
			using var state = baseRepository.GetState(server);
			foreach (var item in state.Object) repository.AddLifetime(item);
		}
	}
}
