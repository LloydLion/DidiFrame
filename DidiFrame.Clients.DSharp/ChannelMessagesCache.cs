﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Clients.DSharp
{
	public class ChannelMessagesCache
	{
		/// <summary>
		/// Limit of messages that channel will cache
		/// </summary>
		public const int MessagesLimit = 10;
		/// <summary>
		/// Limit of message that channel will provide.
		/// All messages out the limit is not exist
		/// </summary>
		public const int MessagesIdLimit = 70;


		private readonly Dictionary<TextChannelBase, CacheItem> messages;


		public ChannelMessagesCache()
		{

		}


		public void AddMessage(Message msg)
		{
			var cache = messages[msg.BaseChannel];

			cache.Messages.Add(msg);
			cache.MessageIds.Add(msg.Id);

			if (cache.Messages.Count > MessagesLimit)
				cache.Messages.RemoveAt(0);
			if (cache.MessageIds.Count > MessagesIdLimit)
				cache.MessageIds.RemoveAt(0);
		}

		public Message DeleteMessage(ulong msgId, TextChannelBase textChannel)
		{
			var cache = messages[textChannel];

			var message = cache.Messages.Single(s => s.Id == msgId);
			cache.Messages.Remove(message);
			cache.MessageIds.Remove(msgId);
			return message;
		}

		public IReadOnlyCollection<Message> GetMessages(TextChannelBase textChannel) => messages[textChannel].Messages;

		public bool HasMessage(ulong id, TextChannelBase textChannel) => messages[textChannel].MessageIds.Contains(id);

		public async Task InitChannelAsync(TextChannelBase textChannel)
		{
			messages.TryAdd(textChannel, new());
			var cache = messages[textChannel];

			var msgs = await textChannel.BaseChannel.GetMessagesAsync(MessagesIdLimit);
			foreach (var item in msgs.Take(MessagesLimit))
				AddMessage(new(item, textChannel, MessageConverter.ConvertDown(item)));
			 cache.MessageIds.AddRange(msgs.Skip(MessagesLimit).Select(s => s.Id));
		}

		public void DeleteChannelCache(TextChannelBase textChannel)
		{
			messages.Remove(textChannel);
		}

		public Message? GetReadyMessage(ulong id, TextChannelBase textChannelBase) => messages[textChannelBase].Messages.SingleOrDefault(s => s.Id == id);


		private struct CacheItem
		{
			public CacheItem()
			{
				Messages = new();
				MessageIds = new();
			}
			

			public List<Message> Messages { get; }

			public List<ulong> MessageIds { get; }
		}
	}
}