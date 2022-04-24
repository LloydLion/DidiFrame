namespace DidiFrame.Interfaces
{
	public interface IServer : IEquatable<IServer>
	{
		public IReadOnlyCollection<IMember> GetMembers();

		public IMember GetMember(ulong id);

		public IReadOnlyCollection<IChannelCategory> GetCategories();

		public IChannelCategory GetCategory(ulong? id);

		public IReadOnlyCollection<IChannel> GetChannels();

		public IChannel GetChannel(ulong id);

		public IReadOnlyCollection<IRole> GetRoles();

		public IRole GetRole(ulong id);


		public IClient Client { get; }

		public string Name { get; }

		public ulong Id { get; }
	}
}
