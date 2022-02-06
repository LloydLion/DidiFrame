using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.DSharpAdapter
{
	internal class ChannelCategory : IChannelCategory
	{
		private readonly DiscordChannel? category;
		private readonly DiscordGuild guild;
		private readonly Server server;


		public string? Name => category?.Name;

		public IReadOnlyCollection<IChannel> Channels =>
			guild.GetChannelsAsync().Result.Where(s => s.Parent == category).Select(s => Channel.Construct(s, server)).ToArray();

		public ulong? Id => category?.Id;

		public IServer Server => server;


		public ChannelCategory(DiscordChannel category, Server server)
		{
			this.category = category;
			this.server = server;
			this.guild = category.Guild;
		}

		public ChannelCategory(DiscordGuild guild, Server server)
		{
			this.guild = guild;
			this.server = server;
		}


		public bool Equals(IServerEntity? other) => Equals(other as ChannelCategory);

		public bool Equals(IChannelCategory? other) => other is ChannelCategory category && category.Id == Id;
	}
}
