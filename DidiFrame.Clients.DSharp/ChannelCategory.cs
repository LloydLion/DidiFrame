using DidiFrame.Entities;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IChannelCategory
	/// </summary>
	public class ChannelCategory : IChannelCategory
	{
		private readonly DiscordChannel? category;
		private readonly Server server;


		/// <inheritdoc/>
		public string? Name => category?.Name;

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannel> Channels =>
			server.GetChannels().Where(s => s.Category == this).ToArray();

		/// <inheritdoc/>
		public ulong? Id => category?.Id;

		/// <inheritdoc/>
		public IServer Server => server;


		/// <summary>
		/// Create new instance of DidiFrame.Clients.DSharp.ChannelCategory that based on real category
		/// </summary>
		/// <param name="category">Base DiscordChannel that will be used as category</param>
		/// <param name="server">Owner server wrap object</param>
		/// <exception cref="ArgumentException">If base channel's server and transmited server wrap are different</exception>
		public ChannelCategory(DiscordChannel category, Server server)
		{
			if (category.GuildId != server.Id)
				throw new ArgumentException("Base channel's server and transmited server wrap are different", nameof(server));

				this.category = category;
			this.server = server;
		}

		/// <summary>
		/// Create new instance of DidiFrame.Clients.DSharp.ChannelCategory that will be used as global category
		/// </summary>
		/// <param name="server">Owner server wrap object</param>
		public ChannelCategory(Server server)
		{
			this.server = server;
		}


		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => Equals(other as ChannelCategory);

		/// <inheritdoc/>
		public bool Equals(IChannelCategory? other) => other is ChannelCategory category && category.Id == Id;

		/// <inheritdoc/>
		public async Task<IChannel> CreateChannelAsync(ChannelCreationModel creationModel) =>
			Channel.Construct(await server.SourceClient.DoSafeOperationAsync(() => server.Guild.CreateChannelAsync(creationModel.Name, creationModel.ChannelType.GetDSharp(), category)), server);
	}
}
