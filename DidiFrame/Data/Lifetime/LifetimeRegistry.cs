namespace DidiFrame.Data.Lifetime
{
	/// <summary>
	/// Simple implementation of DidiFrame.Data.Lifetime.ILifetimesRegistry`2
	/// </summary>
	/// <typeparam name="TLifetime">Type of target lifetime</typeparam>
	/// <typeparam name="TBase">Type of base object of that lifetime</typeparam>
	public class LifetimeRegistry<TLifetime, TBase> : ILifetimesRegistry where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		private readonly IServersLifetimesRepository<TLifetime, TBase> repository;
		private readonly IServersStatesRepository<ICollection<TBase>> baseRepository;


		/// <summary>
		/// Creates new instance of DidiFrame.Data.Lifetime.LifetimeRegistry`2
		/// </summary>
		/// <param name="repository">Repository for push lifetimes</param>
		/// <param name="baseRepository">Repository to provide states</param>
		public LifetimeRegistry(IServersLifetimesRepository<TLifetime, TBase> repository, IServersStatesRepository<ICollection<TBase>> baseRepository)
		{
			this.repository = repository;
			this.baseRepository = baseRepository;
		}


		public void LoadAndRunAll(IServer server)
		{
			using var state = baseRepository.GetState(server);
			for (int i = 0; i < state.Object.Count; i++)
				repository.AddLifetime(state.Object.ElementAt(i));
		}
	}
}
