namespace DidiFrame.Data.Lifetime
{
	public interface IServersLifetimesRepository<TLifetime, TBase> where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		public TLifetime AddLifetime(TBase baseObject);

		public TLifetime GetLifetime(TBase baseObject);

		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server);
	}
}
