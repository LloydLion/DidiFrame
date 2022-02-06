namespace CGZBot3.Interfaces
{
	internal interface IRole : IServerEntity
	{
		public Permissions Permissions { get; }

		public string Name { get; }

		public ulong Id { get; }
	}
}
