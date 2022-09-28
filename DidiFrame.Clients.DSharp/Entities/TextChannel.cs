using DidiFrame.Clients;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.ITextChannel
	/// </summary>
	public class TextChannel : TextChannelBase, ITextChannel
	{
		private readonly ServerWrap server;


		/// <summary>
		/// Creates new instrance 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="channel"></param>
		/// <param name="server"></param>
		public TextChannel(ulong id, ObjectSourceDelegate<DiscordChannel> channel, ServerWrap server) : base(id, channel, server)
		{
			this.server = server;
		}


		/// <inheritdoc/>
		public IReadOnlyCollection<ITextThread> GetThreads() => server.GetThreadsFor(this);
	}
}
