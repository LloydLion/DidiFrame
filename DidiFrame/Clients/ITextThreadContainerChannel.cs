namespace DidiFrame.Clients
{
	/// <summary>
	/// Channel that contains text threads
	/// </summary>
	public interface ITextThreadContainerChannel : IChannel
	{
		/// <summary>
		/// Provides all threads for this channel
		/// </summary>
		/// <returns></returns>
		public IReadOnlyCollection<ITextThread> GetThreads();
	}
}
