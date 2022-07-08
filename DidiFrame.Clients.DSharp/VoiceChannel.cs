using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IVoiceChannel
	/// </summary>
	public class VoiceChannel : TextChannelBase, IVoiceChannel
	{
		private readonly Server server;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.VoiceChannel
		/// </summary>
		/// <param name="channel">Base DiscordChannel from DSharp</param>
		/// <param name="server">Owner server wrap object</param>
		/// <exception cref="ArgumentException">If channel isn't voice</exception>
		/// <exception cref="ArgumentException">If base channel's server and transmited server wrap are different</exception>
		public VoiceChannel(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server) : base(id, channel, server)
		{
			this.server = server;
		}


		/// <inheritdoc/>
		public IReadOnlyCollection<IMember> ConnectedMembers => AccessBase().Users.Select(s => server.GetMember(s.Id)).ToArray();
	}
}
