namespace CGZBot3.Interfaces
{
	internal interface IChannel : IServerEntity, IEquatable<IChannel>
	{
		public string Name { get; }

		public string Id { get; }

		public IChannelCategory Category { get; }
	}
}