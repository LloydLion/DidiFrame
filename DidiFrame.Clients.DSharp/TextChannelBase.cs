﻿using DidiFrame.Entities;
using DidiFrame.Entities.Message;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DidiFrame.Utils;
using DSharpPlus.Entities;
using System.Threading.Channels;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.ITextChannel
	/// </summary>
	public class TextChannelBase : Channel, ITextChannelBase
	{
		private readonly ObjectSourceDelegate<DiscordChannel> channel;
		private readonly Server server;


		/// <inheritdoc/>
		public event MessageSentEventHandler? MessageSent;

		/// <inheritdoc/>
		public event MessageDeletedEventHandler? MessageDeleted;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.TextChannel
		/// </summary>
		/// <param name="channel">Base DiscordChannel from DSharp</param>
		/// <param name="server">Owner server object wrap</param>
		/// <exception cref="ArgumentException">If channel is not text (or text compatible)</exception>
		/// <exception cref="ArgumentException">If base channel's server and transmited server wrap are different</exception>
		public TextChannelBase(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server) : base(id, channel, server)
		{
			this.channel = channel;
			this.server = server;
		}

		public TextChannelBase(ulong id, ObjectSourceDelegate<DiscordChannel> channel, Server server, ObjectSourceDelegate<ChannelCategory> targetCategory) : base(id, channel, server, targetCategory)
		{
			this.channel = channel;
			this.server = server;
		}


		/// <inheritdoc/>
		public IMessage GetMessage(ulong id)
		{
			var obj = AccessBase();

			if (server.GetMessagesCache().HasMessage(id, obj) == false)
				throw new ArgumentException("No such message with same id", nameof(id));

			return new Message(id, () => server.GetMessagesCache().GetMessage(id, AccessBase()), this);
		}

		/// <inheritdoc/>
		public IReadOnlyList<IMessage> GetMessages(int count = -1)
		{
			var obj = AccessBase();

			var messages = server.GetMessagesCache().GetMessages(obj);
			if (count == -1) return castCollection(messages);
			else
			{
				var taken = messages.AsEnumerable().Reverse().Take(Math.Min(count, messages.Count)).ToArray();
				return castCollection(taken);
			}


			IReadOnlyList<IMessage> castCollection(IReadOnlyList<DiscordMessage> messages)
			{
				return messages.Select(s =>
				{
					var id = s.Id;
					return new Message(id, () => server.GetMessagesCache().GetMessage(id, AccessBase()), this);
				}).ToArray();
			}
		}

		/// <inheritdoc/>
		public bool HasMessage(ulong id)
		{
			var obj = AccessBase();
			return server.GetMessagesCache().HasMessage(id, obj);
		}


		/// <inheritdoc/>
		public async Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			var obj = AccessBase();

			var builder = MessageConverter.ConvertUp(messageSendModel);

			var msg = await server.SourceClient.DoSafeOperationAsync(async () =>
			{
				var msg = await obj.SendMessageAsync(builder);
				server.CacheMessage(msg);
				return server.GetMessagesCache().GetMessage(msg.Id, obj);
			}, new(Client.ChannelName, Id, Name));


			var id = msg.Id;
			return new Message(id, () => server.GetMessagesCache().GetMessage(id, AccessBase()), this);
		}
	}
}
