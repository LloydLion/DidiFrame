using DidiFrame.Utils.RoutedEvents;

namespace DidiFrame.Clients
{
	public interface IClient : IRoutedEventObject
	{
		public ValueTask ConnectAsync();

		public Task AwaitForExit();

		public IReadOnlyCollection<IServer> ListServers();
	}
}
