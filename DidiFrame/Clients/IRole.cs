namespace DidiFrame.Clients
{
	/// <summary>
	/// Represents a discord role on server
	/// </summary>
	public interface IRole : IServerEntity, IEquatable<IRole>, IMentionable
	{
		/// <summary>
		/// Permissions that role grants
		/// </summary>
		public Permissions Permissions { get; }

		/// <summary>
		/// Name of the role
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Id of the role
		/// </summary>
		public ulong Id { get; }
	}
}
