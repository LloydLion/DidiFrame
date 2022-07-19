namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Repository that provides the servers' lifetimes
	/// </summary>
	/// <typeparam name="TLifetime">Type of target lifetime</typeparam>
	/// <typeparam name="TBase">Type of base object of that lifetime</typeparam>
	public interface IServersLifetimesRepository<TLifetime, TBase> where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		/// <summary>
		/// Creates and registers new lifetime by its state object using DidiFrame.Lifetimes.ILifetimeFactory`2
		/// </summary>
		/// <param name="baseObject">Internal state object</param>
		/// <returns>New lifetime object</returns>
		public TLifetime AddLifetime(TBase baseObject);

		/// <summary>
		/// Gets lifetime by associated with it state object
		/// </summary>
		/// <param name="baseObject"></param>
		/// <returns>Found lifetime object</returns>
		public TLifetime GetLifetime(TBase baseObject);

		/// <summary>
		/// Gets all lifetime at given server
		/// </summary>
		/// <param name="server">Target server</param>
		/// <returns>Lifetime list</returns>
		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server);
	}
}
