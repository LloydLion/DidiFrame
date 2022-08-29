using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	public class TextThread : TextChannelBase, ITextThread
	{
		private readonly TextChannel channel;


		internal TextThread(string name, TextChannel channel) : base(name, channel.BaseCategory)
		{
			this.channel = channel;
		}


		public ITextChannel Parent => GetIfExist(channel);

		public TextChannel BaseParent => GetIfExist(channel);
	}
}
