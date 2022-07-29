namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Loader for lifetimes that can restore all lifetimes in server
	/// </summary>
	public interface ILifetimesLoader
	{
		/// <summary>
		/// Restores all lifetime in server
		/// </summary>
		/// <param name="server">Target server</param>
		public void RestoreLifetimes(IServer server);
	}
}
