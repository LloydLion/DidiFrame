using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test IForumChannel implementation
	/// </summary>
	public class ForumChannel : Channel, IForumChannel
	{
		private readonly ThreadsContainer threads = new();


		internal ForumChannel(string name, ChannelCategory category) : base(name, category)
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
