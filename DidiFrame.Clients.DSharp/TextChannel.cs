using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	public class TextChannel : TextChannelBase, ITextChannel
	{
		private readonly Server server;


		public TextChannel(DiscordChannel channel, Server server) : base(channel, server)
		{
			this.server = server;
		}


		public IReadOnlyCollection<ITextThread> GetThreads() => server.GetThreadsFor(this);
	}
}
