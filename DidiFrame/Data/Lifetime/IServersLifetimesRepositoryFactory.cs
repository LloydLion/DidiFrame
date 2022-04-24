namespace DidiFrame.Data.Lifetime
{
	public interface IServersLifetimesRepositoryFactory
	{
		public IServersLifetimesRepository<TLifetime, TBase> Create<TLifetime, TBase>(string stateKey)
			where TLifetime : ILifetime<TBase>
			where TBase : class, ILifetimeBase;
	}
}
