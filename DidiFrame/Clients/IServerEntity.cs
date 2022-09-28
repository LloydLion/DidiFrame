namespace DidiFrame.Clients
{
	/// <summary>
	/// Represents any server's entity
	/// </summary>
	public interface IServerEntity : IEquatable<IServerEntity>
	{
		/// <summary>
		/// Server that contains it
		/// </summary>
		public IServer Server { get; }

		/// <summary>
		/// If enity still exist
		/// </summary>
		public bool IsExist { get; }
	}
}
