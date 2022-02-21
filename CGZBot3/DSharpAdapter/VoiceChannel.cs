using DSharpPlus;
using DSharpPlus.Entities;
using ChannelType = DSharpPlus.ChannelType;

namespace CGZBot3.DSharpAdapter
{
	internal class VoiceChannel : Channel, IVoiceChannel
	{
		private readonly DiscordChannel channel;
		private readonly Server server;


		public VoiceChannel(DiscordChannel channel, Server server) : base(channel, server)
		{
			if (!new[] { ChannelType.Text, ChannelType.Group, ChannelType.News, ChannelType.Private }.Contains(channel.Type))
			{
				throw new InvalidOperationException("Channel must be text");
			}

			this.channel = channel;
			this.server = server;
		}


		public IReadOnlyCollection<IMember> ConnectedMembers => channel.Users.Select(s => server.GetMemberAsync(s.Id).Result).ToArray();
	}
}
