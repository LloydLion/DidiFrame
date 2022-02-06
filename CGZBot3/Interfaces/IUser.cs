namespace CGZBot3.Interfaces
{
	internal interface IUser : IEquatable<IUser>
	{
		public string UserName { get; }

		public ulong Id { get; }

		public IClient Client { get; }
	}
}
