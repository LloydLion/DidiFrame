namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Statemachine-based lifetime state object that represents all lifetime's state and can be saved to a server state
	/// </summary>
	/// <typeparam name="TState">Internal statemachine state type</typeparam>
	public interface IStateBasedLifetimeBase<TState> : ILifetimeBase
	{
		/// <summary>
		/// Settable internal statemachine state
		/// </summary>
		public TState State { get; set; }
	}
}
