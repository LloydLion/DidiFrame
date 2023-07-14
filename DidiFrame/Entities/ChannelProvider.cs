using DidiFrame.Utils.RoutedEvents;

namespace DidiFrame.Entities
{
	public class ChannelProvider : IChannel
	{
		private readonly IChannel channel;


		public ChannelProvider(IServer server, ulong targetId)
		{
			channel = server.GetChannel<IChannel>(targetId);
		}


		public ICategory Category => channel.Category;

		public string Name => channel.Name;

		public IServer Server => channel.Server;

		public bool IsExists => channel.IsExists;

		public ulong Id => channel.Id;

		public DateTimeOffset CreationTimeStamp => channel.CreationTimeStamp;


		public void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs => channel.AddListener(routedEvent, handler);

		public void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs => channel.RemoveListener(routedEvent, handler);

		public ValueTask ChangeCategoryAsync(ICategory category) => channel.ChangeCategoryAsync(category);

		public ValueTask DeleteAsync() => channel.DeleteAsync();

		public IChannelPermissions ManagePermissions() => channel.ManagePermissions();

		public ValueTask RenameAsync(string newName) => channel.RenameAsync(newName);

		public TChannel RepresentAs<TChannel>() where TChannel : notnull, IChannel
		{
			return Server.GetChannel<TChannel>(Id);
		}
	}
}
