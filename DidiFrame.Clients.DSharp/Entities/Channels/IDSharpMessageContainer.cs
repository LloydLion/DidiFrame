using DidiFrame.Utils.RoutedEvents;

namespace DidiFrame.Clients.DSharp.Entities.Channels
{
	public interface IDSharpMessageContainer : IMessageContainer
	{
		public void AttachNode(RoutedEventTreeNode node);
	}
}
