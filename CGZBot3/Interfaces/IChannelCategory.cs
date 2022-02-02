namespace CGZBot3.Interfaces
{
	internal interface IChannelCategory : IServerEntity, IEquatable<IChannelCategory>
	{
		public string? Name { get; }

		public IReadOnlyCollection<IChannel> Channels { get; }

		public string Id { get; }
	}
}
