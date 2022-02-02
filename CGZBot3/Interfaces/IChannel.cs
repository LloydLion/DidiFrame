namespace CGZBot3.Interfaces
{
	internal interface IChannel
	{
		public string Name { get; }

		public IChannelCategory Category { get; }
	}
}