namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// The lifeitme object of bot that can be "alive" outside of any commands and events handlers
	/// </summary>
	/// <typeparam name="TBase">Type that represents all lifetime object state and can be saved to a server state</typeparam>
	public interface ILifetime<TBase> : IDisposable where TBase : class, ILifetimeBase
	{
		/// <summary>
		/// Starts lifetime processing
		/// </summary>
		/// <param name="initialBase">Initial base for lifetime, cannot be saved in lifetime</param>
		/// <param name="context">Context where lifetime runs</param>
		public void Run(TBase initialBase, ILifetimeContext<TBase> context);

		public void Update();

		public void Destroy();
	}
}