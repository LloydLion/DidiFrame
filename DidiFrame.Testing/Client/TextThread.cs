using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test ITextThread implementation
	/// </summary>
	public class TextThread : TextChannelBase, ITextThread
	{
		private readonly ITextThreadContainerChannel parentChannel;
		private readonly ThreadsContainer parentContainer;


		internal TextThread(string name, ITextThreadContainerChannel parentChannel, ThreadsContainer parentContainer) : base(name, (ChannelCategory)parentChannel.Category)
		{
			this.parentChannel = parentChannel;
			this.parentContainer = parentContainer;
		}

		internal TextThread(string name, TextChannel parentChannel) : this(name, parentChannel, parentChannel.Threads) { }

		internal TextThread(string name, ForumChannel parentChannel) : this(name, parentChannel, parentChannel.Threads) { }


		/// <inheritdoc/>
		public ITextThreadContainerChannel Parent => GetIfExist(parentChannel);

		/// <summary>
		/// Base parent container
		/// </summary>
		public ThreadsContainer BaseContainer => GetIfExist(parentContainer);
	}
}
