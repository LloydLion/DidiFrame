namespace DidiFrame.Data.Lifetime
{
	/// <summary>
	/// The lifeitme object of bot that can be "alive" outside of any commands and events handlers
	/// </summary>
	/// <typeparam name="TBase">Type that represents all lifetime object state and can be saved to a server state</typeparam>
	public interface ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		/// <summary>
		/// Starts lifetime processing
		/// </summary>
		/// <param name="updater">Updater to notify about internal state changed</param>
		public void Run(ILifetimeStateUpdater<TBase> updater);

		/// <summary>
		/// Gets internal state object clone
		/// </summary>
		/// <returns>Clone object</returns>
		public TBase GetBaseClone();
	}
}