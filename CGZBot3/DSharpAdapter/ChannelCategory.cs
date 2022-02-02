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
		private readonly DiscordClient client;
		private readonly DiscordGuild guild;

		public string? Name => category?.Name;

		public IReadOnlyCollection<IChannel> Channels =>
			guild.GetChannelsAsync().Result.Where(s => s.Parent == category).Select(s => new Channel(s, client)).ToArray();

		public string Id => category?.Id.ToString() ?? $"server {guild.Id}";

		public IServer Server => new Server(guild, client);


		public ChannelCategory(DiscordChannel category, DiscordClient client)
		{
			this.category = category;
			this.client = client;
			this.guild = category.Guild;
		}

		public ChannelCategory(DiscordGuild guild, DiscordClient client)
		{
			this.guild = guild;
			this.client = client;
		}


		public bool Equals(IServerEntity? other) => Equals(other as ChannelCategory);

		public bool Equals(IChannelCategory? other) => other is ChannelCategory category && category.Id == Id;
	}
}
