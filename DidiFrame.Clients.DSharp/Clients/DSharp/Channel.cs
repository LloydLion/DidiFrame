using DidiFrame.Entities;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	public class Channel : IChannel
	{
		private readonly DiscordChannel channel;
		private readonly Server server;

		public string Name => channel.Name;

		public ulong Id => channel.Id;

		public IChannelCategory Category => server.GetCategory(channel.ParentId);

		public IServer Server => server;

		public DiscordChannel BaseChannel => channel;

		public bool IsExist => server.HasChannel(this);


		public Channel(DiscordChannel channel, Server server)
		{
			this.channel = channel;
			this.server = server;
		}


		public bool Equals(IServerEntity? other) => Equals(other as Channel);

		public bool Equals(IChannel? other) => other is Channel channel && channel.Id == Id;


		public static Channel Construct(DiscordChannel channel, Server server)
		{
			return channel.Type.GetAbstract() switch
			{
				ChannelType.TextCompatible => new TextChannel(channel, server),
				ChannelType.Voice => new VoiceChannel(channel, server),
				_ => new Channel(channel, server)
			};
		}

		public Task DeleteAsync() => server.SourceClient.DoSafeOperationAsync(() => channel.DeleteAsync(), new(Client.ChannelName, Id, Name));
	}
}
