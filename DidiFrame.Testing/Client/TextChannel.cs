using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test ITextChannel implementation
	/// </summary>
	public class TextChannel : TextChannelBase, ITextChannel
	{
		private readonly ThreadsContainer threads = new();


		internal TextChannel(string name, ChannelCategory category) : base(name, category)
		{

		}


		/// <summary>
		/// Container for channel's threads
		/// </summary>
		public ThreadsContainer Threads => GetIfExist(threads);


		/// <inheritdoc/>
		public IReadOnlyCollection<ITextThread> GetThreads() => GetIfExist(threads).GetThreads();
	}
}