namespace DidiFrame.Lifetimes
{
	public interface ILifetimesRegistry<TLifetime, TBase> : ILifetimesLoader where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		public TLifetime RegistryLifetime(TBase baseObject);

		public TLifetime GetLifetime(IServer server, Guid baseGuid);

		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server);
	}
}
