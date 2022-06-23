using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IVoiceChannel
	/// </summary>
	public class VoiceChannel : Channel, IVoiceChannel
	{
		private readonly DiscordChannel channel;
		private readonly Server server;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.VoiceChannel
		/// </summary>
		/// <param name="channel">Base DiscordChannel from DSharp</param>
		/// <param name="server">Owner server wrap object</param>
		/// <exception cref="ArgumentException">If channel isn't voice</exception>
		/// <exception cref="ArgumentException">If base channel's server and transmited server wrap are different</exception>
		public VoiceChannel(DiscordChannel channel, Server server) : base(channel, server)
		{
			if (channel.Type.GetAbstract() != Entities.ChannelType.Voice)
			{
				throw new ArgumentException("Channel must be voice", nameof(channel));
			}

			this.channel = channel;
			this.server = server;
		}


		/// <inheritdoc/>
		public IReadOnlyCollection<IMember> ConnectedMembers => channel.Users.Select(s => server.GetMember(s.Id)).ToArray();
	}
}
