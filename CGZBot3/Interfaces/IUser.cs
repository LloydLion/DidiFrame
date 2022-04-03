namespace CGZBot3.Interfaces
{
	public interface IUser : IEquatable<IUser>
	{
		public string UserName { get; }

		public ulong Id { get; }

		public IClient Client { get; }

		public string Mention { get; }
	}
}
