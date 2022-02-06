namespace CGZBot3.Interfaces
{
	internal interface IChannel : IServerEntity, IEquatable<IChannel>
	{
		public string Name { get; }

		public ulong Id { get; }

		public IChannelCategory Category { get; }
	}
}