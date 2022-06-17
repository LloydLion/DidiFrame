using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	internal class VoiceChannel : Channel, IVoiceChannel
	{
		private readonly DiscordChannel channel;
		private readonly Server server;


		public VoiceChannel(DiscordChannel channel, Server server) : base(channel, server)
		{
			if (channel.Type.GetAbstract() != Entities.ChannelType.Voice)
			{
				throw new InvalidOperationException("Channel must be voice");
			}

			this.channel = channel;
			this.server = server;
		}


		public IReadOnlyCollection<IMember> ConnectedMembers => channel.Users.Select(s => server.GetMember(s.Id)).ToArray();
	}
}
