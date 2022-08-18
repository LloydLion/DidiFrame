using DidiFrame.Client;
using DSharpPlus.Entities;

namespace DidiFrame.Client.DSharp.Entities
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.ITextChannel
	/// </summary>
	public class TextChannel : TextChannelBase, ITextChannel
	{
		private readonly Server server;


		/// <summary>
		/// Creates new instrance 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="channel"></param>
		/// <param name="server"></param>
		public TextChannel(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server) : base(id, channel, server)
		{
			this.server = server;
		}


		/// <inheritdoc/>
		public IReadOnlyCollection<ITextThread> GetThreads() => server.GetThreadsFor(this);
	}
}
