namespace DidiFrame.Interfaces
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

		public bool IsExist { get; }
	}
}
