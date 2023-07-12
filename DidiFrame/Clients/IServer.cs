using DidiFrame.Utils.RoutedEvents;

namespace DidiFrame.Clients
{
	/// <summary>
	/// Represents discord server
	/// </summary>
	public interface IServer : IDiscordObject
	{
		public IClient Client { get; }

		public ServerStatus Status { get; }


		public IReadOnlyCollection<IMember> ListMembers();

		public IMember GetMember(ulong id);

		public IReadOnlyCollection<IRole> ListRoles();

		public IRole GetRole(ulong id);

		public IReadOnlyCollection<ICategory> ListCategories();

		public ICategory GetCategory(ulong id);

		public IReadOnlyCollection<ChannelProvider> ListChannels();

		public TChannel GetChannel<TChannel>(ulong id) where TChannel : notnull, IChannel;

		public IServerPermissions ManagePermissions();

		public void DispatchTask(IServerTask task);


		public static readonly RoutedEvent<ServerEventArgs> ServerCreatedPre = new(typeof(IServer), nameof(ServerCreatedPre), RoutedEvent.PropagationDirection.Bubbling);

		public static readonly RoutedEvent<ServerEventArgs> ServerCreatedPost = new(typeof(IServer), nameof(ServerCreatedPost), RoutedEvent.PropagationDirection.Bubbling);

		public static readonly RoutedEvent<ServerEventArgs> ServerRemovedPre = new(typeof(IServer), nameof(ServerRemovedPre), RoutedEvent.PropagationDirection.Bubbling);

		public static readonly RoutedEvent<ServerEventArgs> ServerRemovedPost = new(typeof(IServer), nameof(ServerRemovedPost), RoutedEvent.PropagationDirection.Bubbling);


		public class ServerEventArgs : EventArgs
		{
			public ServerEventArgs(IServer server)
			{
				Server = server;
			}


			public IServer Server { get; }
		}
	}
}
