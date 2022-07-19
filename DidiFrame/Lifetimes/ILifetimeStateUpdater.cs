namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Object that lifetime must notify about state object changings
	/// </summary>
	/// <typeparam name="TBase">Type of lifeitme state object</typeparam>
	public interface ILifetimeStateUpdater<TBase> where TBase : class, ILifetimeBase
	{
		/// <summary>
		/// Updates or creates record about lifetime state
		/// </summary>
		/// <param name="lifetime">Lifetime itself</param>
		public void Update(ILifetime<TBase> lifetime);

		/// <summary>
		/// Deletes record about lifetime state
		/// </summary>
		/// <param name="lifetime">Lifetime itself</param>
		public void Finish(ILifetime<TBase> lifetime);


		/// <summary>
		/// Event that fired when lifetime finished to execute
		/// </summary>
		public event Action<ILifetime<TBase>>? Finished;
	}
}
