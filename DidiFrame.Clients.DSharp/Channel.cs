﻿using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IChannel
	/// </summary>
	public class Channel : IChannel
	{
		private readonly ObjectSourceDelegate<DiscordChannel> channel;
		private readonly Server server;
		private readonly ObjectSourceDelegate<ChannelCategory> targetCategory;


		/// <inheritdoc/>
		public string Name => AccessBase().Name;

		/// <inheritdoc/>
		public ulong Id { get; }

		/// <inheritdoc/>
		public IChannelCategory Category
		{
			get
			{
				var obj = targetCategory();
				if (obj is null)
					throw new ObjectDoesNotExistException(nameof(Category));
				else return obj;
			}
		}

		/// <inheritdoc/>
		public IServer Server => server;

		/// <inheritdoc/>
		public DiscordChannel BaseChannel => AccessBase();

		/// <summary>
		/// Base server for channel, casted to DidiFrame.Clients.DSharp.Server Server property
		/// </summary>
		public Server BaseServer => server;

		/// <inheritdoc/>
		public bool IsExist => channel() is not null;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.Channel
		/// </summary>
		/// <param name="channel">Base DiscordChannel from DSharp</param>
		/// <param name="server">Owner server wrap object</param>
		public Channel(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server) : this(id, channel, server, () =>
		{
			var obj = channel();
			if (obj is null) return null;
			else return (ChannelCategory)server.GetCategory(obj.ParentId);
		}) { }

		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.Channel with overrided category
		/// </summary>
		/// <param name="channel">Base DiscordChannel from DSharp</param>
		/// <param name="server">Owner server wrap object</param>
		/// <param name="targetCategory">Custom category source</param>
		public Channel(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server, ObjectSourceDelegate<ChannelCategory> targetCategory)
		{
			Id = id;
			this.channel = channel;
			this.server = server;
			this.targetCategory = targetCategory;
		}


		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => Equals(other as Channel);

		/// <inheritdoc/>
		public bool Equals(IChannel? other) => other is Channel channel && channel.IsExist && IsExist && channel.Id == Id;


		/// <inheritdoc/>
		public static Channel Construct(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server)
		{
			var nowChannel = channel();
			if (nowChannel is null) return new Channel(id, channel, server);

			return nowChannel.Type.GetAbstract() switch
			{
				ChannelType.TextCompatible => new TextChannel(id, channel, server),
				ChannelType.Voice => new VoiceChannel(id, channel, server),
				_ => new Channel(id, channel, server)
			};
		}

		/// <inheritdoc/>
		public Task DeleteAsync()
		{
			lock (this)
			{
				var obj = AccessBase();
				return server.SourceClient.DoSafeOperationAsync(() => obj.DeleteAsync(), new(Client.ChannelName, Id, Name));
			}
		}

		protected DiscordChannel AccessBase([CallerMemberName] string nameOfCaller = "")
		{
			var obj = channel();
			if (obj is null)
				throw new ObjectDoesNotExistException(nameOfCaller);
			else return obj;
		}
	}
}
