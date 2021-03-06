namespace DidiFrame.Interfaces
{
	/// <summary>
	/// Represents a discord channel
	/// </summary>
	public interface IChannel : IServerEntity, IEquatable<IChannel>
	{
		/// <summary>
		/// Humanized name of channel
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Id if channel
		/// </summary>
		public ulong Id { get; }

		/// <summary>
		/// Category that contains channel
		/// </summary>
		public IChannelCategory Category { get; }

		/// <summary>
		/// If channel still exists on discord server
		/// </summary>
		public bool IsExist { get; }


		/// <summary>
		/// Deletes channel async
		/// </summary>
		/// <returns>Wait task</returns>
		Task DeleteAsync();
	}
}