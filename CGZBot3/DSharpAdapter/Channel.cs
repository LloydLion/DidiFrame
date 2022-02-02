using DSharpPlus;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class Channel : IChannel
	{
		private readonly DiscordChannel channel;
		private readonly DiscordClient client;

		public string Name => channel.Name;

		public string Id => channel.Id.ToString();

		public IChannelCategory Category => channel.Parent is null ?
			new ChannelCategory(channel.Guild, client) : new ChannelCategory(channel.Parent, client);

		public IServer Server => new Server(channel.Guild, client);


		public Channel(DiscordChannel channel, DiscordClient client)
		{
			this.channel = channel;
			this.client = client;
		}


		public bool Equals(IServerEntity? other) => Equals(other as Channel);

		public bool Equals(IChannel? other) => other is Channel channel && channel.Id == Id;


		public static Channel Construct(DiscordChannel channel, DiscordClient client)
		{
			return channel.Type switch
			{
				ChannelType.Text => new TextChannel(channel, client),
				_ => new Channel(channel, client)
			};
		}
	}
}
