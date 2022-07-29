namespace DidiFrame.Interfaces
{
	/// <summary>
	/// Represents a discord server
	/// </summary>
	public interface IServer : IEquatable<IServer>
	{
		/// <summary>
		/// Event that fires when new message sent
		/// </summary>
		public event MessageSentEventHandler? MessageSent;

		/// <summary>
		/// Event that fires when a message deleted
		/// </summary>
		public event MessageDeletedEventHandler? MessageDeleted;

		/// <summary>
		/// Event that fires when a channel deleted
		/// </summary>
		public event ServerObjectDeletedEventHandler? ChannelDeleted;

		/// <summary>
		/// Event that fires when a member deleted (left)
		/// </summary>
		public event ServerObjectDeletedEventHandler? MemberDeleted;

		/// <summary>
		/// Event that fires when a role deleted
		/// </summary>
		public event ServerObjectDeletedEventHandler? RoleDeleted;

		/// <summary>
		/// Event that fires when a category deleted
		/// </summary>
		public event ServerObjectDeletedEventHandler? CategoryDeleted;

		/// <summary>
		/// Event that fires when a new channel
		/// </summary>
		public event ServerObjectCreatedEventHandler<IChannel>? ChannelCreated;

		/// <summary>
		/// Event that fires when a new member created (joined)
		/// </summary>
		public event ServerObjectCreatedEventHandler<IMember>? MemberCreated;

		/// <summary>
		/// Event that fires when a new role created
		/// </summary>
		public event ServerObjectCreatedEventHandler<IRole>? RoleCreated;

		/// <summary>
		/// Event that fires when a new category created
		/// </summary>
		public event ServerObjectCreatedEventHandler<IChannelCategory>? CategoryCreated;


		/// <summary>
		/// Provides all server's members
		/// </summary>
		/// <returns>Collection of members</returns>
		public IReadOnlyCollection<IMember> GetMembers();

		/// <summary>
		/// Gets member by id
		/// </summary>
		/// <param name="id">Id of member</param>
		/// <returns>Member with given id</returns>
		public IMember GetMember(ulong id);

		/// <summary>
		/// Provides all server's categories
		/// </summary>
		/// <returns>Collection of categories</returns>
		public IReadOnlyCollection<IChannelCategory> GetCategories();

		/// <summary>
		/// Gets category by id
		/// </summary>
		/// <param name="id">Id of category</param>
		/// <returns>Category with given id</returns>
		public IChannelCategory GetCategory(ulong? id);

		/// <summary>
		/// Provides all server's channels
		/// </summary>
		/// <returns>Collection of channels</returns>
		public IReadOnlyCollection<IChannel> GetChannels();

		/// <summary>
		/// Gets channel by id
		/// </summary>
		/// <param name="id">Id of channel</param>
		/// <returns>Channel with given id</returns>
		public IChannel GetChannel(ulong id);

		/// <summary>
		/// Provides all server's roles
		/// </summary>
		/// <returns>Collection of roles</returns>
		public IReadOnlyCollection<IRole> GetRoles();

		/// <summary>
		/// Gets role by id
		/// </summary>
		/// <param name="id">Id of role</param>
		/// <returns>Role with given id</returns>
		public IRole GetRole(ulong id);


		/// <summary>
		/// Client that contains this server
		/// </summary>
		public IClient Client { get; }

		/// <summary>
		/// Name of the server
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Id of the server
		/// </summary>
		public ulong Id { get; }

		/// <summary>
		/// If server closed
		/// </summary>
		public bool IsClosed { get; }
	}
}
