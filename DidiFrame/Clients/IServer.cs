using DidiFrame.Utils.RoutedEvents;

namespace DidiFrame.Clients
{
	/// <summary>
	/// Represents discord server
	/// </summary>
	public interface IServer : IDiscordObject
	{
		public IClient Client { get; }

		bool IsClosed { get; }


		public IReadOnlyCollection<IMember> ListMembers();

		public IMember GetMember(ulong id);

		public IReadOnlyCollection<IRole> ListRoles();

		public IRole GetRole(ulong id);

		public IReadOnlyCollection<ICategory> ListCategories();

		public ICategory GetCategory(ulong id);

		public IReadOnlyCollection<ChannelProvider> ListChannels();

		public TChannel GetChannel<TChannel>(ulong id) where TChannel : notnull, IChannel;

		public IServerPermissions ManagePermissions();


		public static readonly RoutedEvent<ServerEventArgs> ServerCreated = new(typeof(IServer), nameof(ServerCreated), RoutedEvent.PropagationDirection.Bubbling);

		public static readonly RoutedEvent<ServerEventArgs> ServerRemoved = new(typeof(IServer), nameof(ServerRemoved), RoutedEvent.PropagationDirection.Bubbling);


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
