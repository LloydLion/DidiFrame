namespace DidiFrame.Interfaces
{
	/// <summary>
	/// Represents a discord server
	/// </summary>
	public interface IServer : IEquatable<IServer>
	{
		public event MessageSentEventHandler MessageSent;

		public event MessageDeletedEventHandler MessageDeleted;


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
	}
}
