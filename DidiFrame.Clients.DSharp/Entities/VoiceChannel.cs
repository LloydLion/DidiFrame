using DidiFrame.Client;
using DSharpPlus.Entities;

namespace DidiFrame.Client.DSharp.Entities
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
		/// <param name="id">Id of channel</param>
		/// <param name="channel">Base DiscordChannel from DSharp source</param>
		/// <param name="server">Owner server wrap object</param>
		public VoiceChannel(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server) : base(id, channel, server)
		{
			this.server = server;
		}


		/// <inheritdoc/>
		public IReadOnlyCollection<IMember> ConnectedMembers => AccessBase().Users.Select(s => server.GetMember(s.Id)).ToArray();
	}
}
