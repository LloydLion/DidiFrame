namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Creates repositories that provide the servers' lifetimes
	/// </summary>
	public interface IServersLifetimesRepositoryFactory
	{
		/// <summary>
		/// Creates new repository
		/// </summary>
		/// <typeparam name="TLifetime">Type of target lifetime</typeparam>
		/// <typeparam name="TBase">Type of base object of that lifetime</typeparam>
		/// <param name="stateKey">Data key that will be used to request base objects from servers' states</param>
		/// <returns>New repository</returns>
		public IServersLifetimesRepository<TLifetime, TBase> Create<TLifetime, TBase>(string stateKey)
			where TLifetime : ILifetime<TBase>
			where TBase : class, ILifetimeBase;
	}
}
