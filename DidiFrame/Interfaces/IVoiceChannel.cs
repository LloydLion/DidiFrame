namespace DidiFrame.Interfaces
{
	/// <summary>
	/// Represents a discord voice channel
	/// </summary>
	public interface IVoiceChannel : IChannel
	{
		/// <summary>
		/// Provides connected to channel members
		/// </summary>
		public IReadOnlyCollection<IMember> ConnectedMembers { get; }
	}
}