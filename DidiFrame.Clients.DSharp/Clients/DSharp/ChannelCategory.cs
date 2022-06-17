using DidiFrame.Entities;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	internal class ChannelCategory : IChannelCategory
	{
		private readonly DiscordChannel? category;
		private readonly Server server;


		public string? Name => category?.Name;

		public IReadOnlyCollection<IChannel> Channels =>
			server.GetChannels().Where(s => s.Category == this).ToArray();

		public ulong? Id => category?.Id;

		public IServer Server => server;


		public ChannelCategory(DiscordChannel category, Server server)
		{
			this.category = category;
			this.server = server;
		}

		public ChannelCategory(Server server)
		{
			this.server = server;
		}


		public bool Equals(IServerEntity? other) => Equals(other as ChannelCategory);

		public bool Equals(IChannelCategory? other) => other is ChannelCategory category && category.Id == Id;

		public async Task<IChannel> CreateChannelAsync(ChannelCreationModel creationModel) =>
			Channel.Construct(await server.SourceClient.DoSafeOperationAsync(() => server.Guild.CreateChannelAsync(creationModel.Name, creationModel.ChannelType.GetDSharp(), category)), server);
	}
}
