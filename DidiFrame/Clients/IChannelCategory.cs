namespace DidiFrame.Clients
{
	/// <summary>
	/// Represents a discord category - container for channels
	/// </summary>
	public interface IChannelCategory : IServerEntity, IEquatable<IChannelCategory>
	{
		/// <summary>
		/// Humanized name of category or null if category is global
		/// </summary>
		public string? Name { get; }

		/// <summary>
		/// Id of category or null if category is global
		/// </summary>
		public ulong? Id { get; }

		/// <summary>
		/// If category is global
		/// </summary>
		public bool IsGlobal => Id == null;

		/// <summary>
		/// Collection of channel that associated with this category
		/// </summary>
		public IReadOnlyCollection<IChannel> Channels { get; }


		/// <summary>
		/// Creates channel in category async
		/// </summary>
		/// <param name="creationModel">Special model for channel creating</param>
		/// <returns>Wait task</returns>
		public Task<IChannel> CreateChannelAsync(ChannelCreationModel creationModel);
	}
}
