namespace CGZBot3.Interfaces
{
	internal interface IServer : IEquatable<IServer>
	{
		public Task<IReadOnlyCollection<IMember>> GetMembersAsync();

		public Task<IMember> GetMemberAsync(string id);

		public Task<IReadOnlyCollection<IChannelCategory>> GetCategoriesAsync();

		public Task<IChannelCategory> GetCategoryAsync(string id);

		public Task<IReadOnlyCollection<IChannel>> GetChannelsAsync();

		public Task<IChannel> GetChannelAsync(string id);


		public IClient Client { get; }

		public string Name { get; }

		public string Id { get; }
	}
}
