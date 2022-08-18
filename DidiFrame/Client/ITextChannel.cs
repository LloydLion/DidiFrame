namespace DidiFrame.Client
{
	/// <summary>
	/// Represents some text-like channel that can contain threads
	/// </summary>
	public interface ITextChannel : ITextChannelBase
	{
		/// <summary>
		/// Provides all threads for this channel
		/// </summary>
		/// <returns></returns>
		public IReadOnlyCollection<ITextThread> GetThreads();
	}
}
