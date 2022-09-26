using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities
{
	/// <summary>
	/// Represents forum channel
	/// </summary>
	public class ForumChannel : Channel, IForumChannel
	{
		private readonly ServerWrap server;


		/// <summary>
		/// Creates new instrance 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="channel"></param>
		/// <param name="server"></param>
		public ForumChannel(ulong id, ObjectSourceDelegate<DiscordChannel> channel, ServerWrap server) : base(id, channel, server)
		{
			this.server = server;
		}


		/// <inheritdoc/>
		public IReadOnlyCollection<ITextThread> GetThreads() => server.GetThreadsFor(this);
	}
}
