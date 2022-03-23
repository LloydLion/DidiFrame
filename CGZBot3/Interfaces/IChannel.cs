namespace CGZBot3.Interfaces
{
	public interface IChannel : IServerEntity, IEquatable<IChannel>
	{
		public string Name { get; }

		public ulong Id { get; }

		public IChannelCategory Category { get; }

		public bool IsExist { get; }


		Task DeleteAsync();
	}
}