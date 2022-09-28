namespace DidiFrame.Clients
{
	/// <summary>
	/// Notifier that provides server created/removed events 
	/// </summary>
	public interface IServersNotify
	{
		/// <summary>
		/// Server created event, fired when bot was added to new server
		/// </summary>
		public event ServerCreatedEventHandler? ServerCreated;

		/// <summary>
		/// Server removed event, fired when bot was removed from server or server was deleted
		/// </summary>
		public event ServerRemovedEventHandler? ServerRemoved;
	}
}
