using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IChannel
	/// </summary>
	public class Channel : IChannel
	{
		private DiscordChannel channel;
		private readonly Server server;
		private readonly Func<ChannelCategory> targetCategoryGetter;


		/// <inheritdoc/>
		public string Name
		{
			get
			{
				lock (this)
				{
					if (IsExist == false)
						throw new ObjectDoesNotExistException(nameof(Name));
					return channel.Name;
				}
			}
		}

		/// <inheritdoc/>
		public ulong Id => channel.Id;

		/// <inheritdoc/>
		public IChannelCategory Category
		{
			get
			{
				lock (this)
				{
					if (IsExist == false)
						throw new ObjectDoesNotExistException(nameof(Category));
					return targetCategoryGetter();
				}
			}
		}

		/// <inheritdoc/>
		public IServer Server => server;

		/// <inheritdoc/>
		public DiscordChannel BaseChannel
		{
			get
			{
				lock (this)
				{
					if (IsExist == false)
						throw new ObjectDoesNotExistException(nameof(BaseChannel));
					return channel;
				}
			}
		}

		/// <summary>
		/// Base server for channel, casted to DidiFrame.Clients.DSharp.Server Server property
		/// </summary>
		public Server BaseServer => server;

		/// <inheritdoc/>
		public bool IsExist => server.HasChannel(this);


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.Channel
		/// </summary>
		/// <param name="channel">Base DiscordChannel from DSharp</param>
		/// <param name="server">Owner server wrap object</param>
		/// <exception cref="ArgumentException">If base channel's server and transmited server wrap are different</exception>
		public Channel(DiscordChannel channel, Server server) : this(channel, server, () => (ChannelCategory)server.GetCategory(channel.ParentId)) { }

		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.Channel with overrided category
		/// </summary>
		/// <param name="channel">Base DiscordChannel from DSharp</param>
		/// <param name="server">Owner server wrap object</param>
		/// <param name="targetCategoryGetter">Custom category source</param>
		/// <exception cref="ArgumentException">If base channel's server and transmited server wrap are different</exception>
		public Channel(DiscordChannel channel, Server server, Func<ChannelCategory> targetCategoryGetter)
		{
			if (channel.GuildId != server.Id)
				throw new ArgumentException("Base channel's server and transmited server wrap are different", nameof(server));

			this.channel = channel;
			this.server = server;
			this.targetCategoryGetter = targetCategoryGetter;
		}


		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => Equals(other as Channel);

		/// <inheritdoc/>
		public bool Equals(IChannel? other) => other is Channel channel && channel.IsExist && IsExist && channel.Id == Id;


		/// <inheritdoc/>
		public static Channel Construct(DiscordChannel channel, Server server, ChannelMessagesCache messages)
		{
			return channel.Type.GetAbstract() switch
			{
				ChannelType.TextCompatible => new TextChannel(channel, server, messages),
				ChannelType.Voice => new VoiceChannel(channel, server, messages),
				_ => new Channel(channel, server)
			};
		}

		/// <inheritdoc/>
		public Task DeleteAsync()
		{
			lock (this)
			{
				if (IsExist == false)
					throw new ObjectDoesNotExistException(nameof(DeleteAsync));
				return server.SourceClient.DoSafeOperationAsync(() => channel.DeleteAsync(), new(Client.ChannelName, Id, Name));
			}
		}


		protected internal virtual void ModifyInternal(DiscordChannel channel)
		{
			lock (this)
			{
				this.channel = channel;
			}
		}
	}
}
