using DidiFrame.Utils.RoutedEvents;

namespace DidiFrame.Clients
{
	/// <summary>
	/// Base discord object interface
	/// </summary>
	public interface IDiscordObject : IRoutedEventObject
	{
		/// <summary>
		/// Unique discord identifier
		/// </summary>
		public ulong Id { get; }

		/// <summary>
		/// Object creation time stamp
		/// </summary>
		public DateTimeOffset CreationTimeStamp { get; }
	}
}
