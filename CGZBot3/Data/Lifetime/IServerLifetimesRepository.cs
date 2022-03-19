namespace CGZBot3.Data.Lifetime
{
	public interface IServerLifetimesRepository<TLifetime, TBase> where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		public TLifetime AddLifetime(TBase baseObject);

		public TLifetime GetLifetime(TBase baseObject);

		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server);
	}
}
