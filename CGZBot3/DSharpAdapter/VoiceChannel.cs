﻿using DSharpPlus;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class VoiceChannel : Channel, IVoiceChannel
	{
		private readonly DiscordChannel channel;
		private readonly Server server;


		public VoiceChannel(DiscordChannel channel, Server server) : base(channel, server)
		{
			if (channel.Type.GetAbstract() != Entities.ChannelType.Voice)
			{
				throw new InvalidOperationException("Channel must be text");
			}

			this.channel = channel;
			this.server = server;
		}


		public IReadOnlyCollection<IMember> ConnectedMembers => channel.Users.Select(s => server.GetMemberAsync(s.Id).Result).ToArray();
	}
}
