namespace CGZBot3.Interfaces
{
	public interface IServer : IEquatable<IServer>
	{
		public Task<IReadOnlyCollection<IMember>> GetMembersAsync();

		public Task<IMember> GetMemberAsync(ulong id);

		public Task<IReadOnlyCollection<IChannelCategory>> GetCategoriesAsync();

		public Task<IChannelCategory> GetCategoryAsync(ulong? id);

		public Task<IReadOnlyCollection<IChannel>> GetChannelsAsync();

		public Task<IChannel> GetChannelAsync(ulong id);

		public Task<IReadOnlyCollection<IRole>> GetRolesAsync();

		public Task<IRole> GetRoleAsync(ulong id);


		public IClient Client { get; }

		public string Name { get; }

		public ulong Id { get; }
	}
}
