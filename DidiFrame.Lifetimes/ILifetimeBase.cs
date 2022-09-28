using DidiFrame.Data.Model;

namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Lifetime state object that represents all lifetime's state and can be saved to a server state
	/// </summary>
	public interface ILifetimeBase : IDataModel
	{
		/// <summary>
		/// Server where lifetime was started
		/// </summary>
		public IServer Server { get; }
	}
}
