namespace DidiFrame.Interfaces
{
	public interface IRole : IServerEntity, IEquatable<IRole>
	{
		public Permissions Permissions { get; }

		public string Name { get; }

		public ulong Id { get; }
	}
}
