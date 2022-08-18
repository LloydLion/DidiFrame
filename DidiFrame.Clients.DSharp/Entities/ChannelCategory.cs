using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp.Entities
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IChannelCategory
	/// </summary>
	public sealed class ChannelCategory : IChannelCategory
	{
		private readonly ObjectSourceDelegate<DiscordChannel>? category;
		private readonly Server server;


		/// <inheritdoc/>
		public string? Name => AccessBase()?.Name;

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannel> Channels => server.GetChannels().Where(s => s.Category == this).ToArray();

		/// <inheritdoc/>
		public ulong? Id { get; }

		/// <inheritdoc/>
		public IServer Server => server;

		/// <inheritdoc/>
		public bool IsExist => category is null || category.Invoke() is not null;


		/// <summary>
		/// Create new instance of DidiFrame.Clients.DSharp.ChannelCategory that based on real category
		/// </summary>
		/// <param name="id">If of category</param>
		/// <param name="category">Base DiscordChannel that will be used as category</param>
		/// <param name="server">Owner server wrap object</param>
		/// <exception cref="ArgumentException">If base channel's server and transmited server wrap are different</exception>
		public ChannelCategory(ulong id, ObjectSourceDelegate<DiscordChannel> category, Server server)
		{
			Id = id;
			this.category = category;
			this.server = server;
		}

		/// <summary>
		/// Create new instance of DidiFrame.Clients.DSharp.ChannelCategory that will be used as global category
		/// </summary>
		/// <param name="server">Owner server wrap object</param>
		public ChannelCategory(Server server)
		{
			Id = null;
			category = null;
			this.server = server;
		}


		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => Equals(other as ChannelCategory);

		/// <inheritdoc/>
		public bool Equals(IChannelCategory? other) => other is ChannelCategory otherCategory && otherCategory.Id == Id;

		/// <inheritdoc/>
		public async Task<IChannel> CreateChannelAsync(ChannelCreationModel creationModel)
		{
			var obj = AccessBase();
			var channel = await server.SourceClient.DoSafeOperationAsync(() => server.Guild.CreateChannelAsync(creationModel.Name, creationModel.ChannelType.GetDSharp(), parent: obj));
			server.CacheChannel(channel);
			return server.GetChannel(channel.Id);
		}

		private DiscordChannel? AccessBase([CallerMemberName] string nameOfCaller = "")
		{
			if (category is null) return null;
			var obj = category();
			if (obj is null)
				throw new ObjectDoesNotExistException(nameOfCaller);
			else return obj;
		}
	}
}
