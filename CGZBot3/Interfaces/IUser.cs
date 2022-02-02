namespace CGZBot3.Interfaces
{
	internal interface IUser : IEquatable<IUser>
	{
		public string UserName { get; }

		public string Id { get; }

		public IClient Client { get; }
	}
}
