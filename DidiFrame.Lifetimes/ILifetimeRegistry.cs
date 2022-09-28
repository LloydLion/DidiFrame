namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Leads lifetime registry, can restore lifetimes and registry new
	/// </summary>
	/// <typeparam name="TLifetime">Target lifetime type</typeparam>
	/// <typeparam name="TBase">A base object type of the lifetime</typeparam>
	public interface ILifetimesRegistry<TLifetime, TBase> : ILifetimesLoader where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		/// <summary>
		/// Registries new lifetime
		/// </summary>
		/// <param name="baseObject">Lifetime's base object</param>
		/// <returns>New lifetime</returns>
		public TLifetime RegistryLifetime(TBase baseObject);

		/// <summary>
		/// Provides lifetime by its id and server
		/// </summary>
		/// <param name="server">Server where need get lifetime</param>
		/// <param name="baseGuid">Lifetime's id</param>
		/// <returns>Target lifetime</returns>
		public TLifetime GetLifetime(IServer server, Guid baseGuid);

		/// <summary>
		/// Provides all lifetime in server
		/// </summary>
		/// <param name="server">Target server</param>
		/// <returns>Collection of lifetimes</returns>
		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server);
	}
}
