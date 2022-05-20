namespace DidiFrame.Data.Lifetime
{
	/// <summary>
	/// Need to restore all lifetime at bot startup from them state objects in servers' states
	/// </summary>
	public interface ILifetimesRegistry
	{
		/// <summary>
		/// Restores and starts all lifetime at server from them state objects. It must be called at start for each server
		/// </summary>
		/// <param name="server">Server where need to restore and start lifetimes</param>
		public void LoadAndRunAll(IServer server);
	}
}
