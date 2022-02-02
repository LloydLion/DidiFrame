namespace CGZBot3.Interfaces
{
	internal interface IChannelCategory
	{
		public string? Name { get; }

		public IReadOnlyCollection<IChannel> Channels { get; }
	}
}
