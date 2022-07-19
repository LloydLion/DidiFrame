namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Need to create lifetimes from them state objects
	/// </summary>
	/// <typeparam name="TLifetime">Type of lifetime associated with TBase</typeparam>
	/// <typeparam name="TBase">Type of lifetime state object</typeparam>
	public interface ILifetimeFactory<TLifetime, TBase>
		where TLifetime : ILifetime<TBase>
		where TBase : class, ILifetimeBase
	{
		/// <summary>
		/// Creates lifetime from its state
		/// </summary>
		/// <param name="baseObject"></param>
		/// <returns></returns>
		public TLifetime Create(TBase baseObject);
	}
}
