namespace CGZBot3.Interfaces
{
	public interface IChannelCategory : IServerEntity, IEquatable<IChannelCategory>
	{
		public string? Name { get; }

		public ulong? Id { get; }

		public bool IsGlobal => Id == null;

		public IReadOnlyCollection<IChannel> Channels { get; }


		public Task<IChannel> CreateChannelAsync(ChannelCreationModel creationModel);
	}
}
