using DidiFrame.Utils.RoutedEvents;

namespace DidiFrame.Clients
{
	public interface IClient : IRoutedEventObject
	{
		public IReadOnlyCollection<IServer> Servers { get; }


		public ValueTask ConnectAsync();

		public Task AwaitForExit();
	}
}
