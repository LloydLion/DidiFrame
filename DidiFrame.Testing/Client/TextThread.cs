using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	public class TextThread : TextChannelBase, ITextThread
	{
		private readonly TextChannel channel;


		public TextThread(string name, TextChannel channel, ChannelCategory category) : base(name, category)
		{
			this.channel = channel;
		}


		public ITextChannel Parent => GetIfExist(channel);

		public TextChannel BaseParent => GetIfExist(channel);
	}
}
