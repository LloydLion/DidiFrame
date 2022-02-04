using DSharpPlus;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class Channel : IChannel
	{
		private readonly DiscordChannel channel;
		private readonly Server server;

		public string Name => channel.Name;

		public string Id => channel.Id.ToString();

		public IChannelCategory Category => channel.Parent is null ?
			new ChannelCategory(channel.Guild, server) : new ChannelCategory(channel.Parent, server);

		public IServer Server => server;


		public Channel(DiscordChannel channel, Server server)
		{
			this.channel = channel;
			this.server = server;
		}


		public bool Equals(IServerEntity? other) => Equals(other as Channel);

		public bool Equals(IChannel? other) => other is Channel channel && channel.Id == Id;


		public static Channel Construct(DiscordChannel channel, Server server)
		{
			return channel.Type switch
			{
				ChannelType.Text => new TextChannel(channel, server),
				_ => new Channel(channel, server)
			};
		}
	}
}
