using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test ITextThread implementation
	/// </summary>
	public class TextThread : TextChannelBase, ITextThread
	{
		private readonly TextChannel channel;


		internal TextThread(string name, TextChannel channel) : base(name, channel.BaseCategory)
		{
			this.channel = channel;
		}


		/// <inheritdoc/>
		public ITextChannel Parent => GetIfExist(channel);

		/// <summary>
		/// Base parent channel
		/// </summary>
		public TextChannel BaseParent => GetIfExist(channel);
	}
}
