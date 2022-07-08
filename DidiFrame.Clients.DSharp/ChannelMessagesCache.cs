using DSharpPlus.Entities;
using System;
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
		/// All messages out the limit is not exist
		/// </summary>
		public const int MessagesLimit = 25;


		private readonly Dictionary<DiscordChannel, CacheItem> messages;


		public ChannelMessagesCache()
		{

		}


		public void AddMessage(DiscordMessage msg)
		{
			lock (Lock(msg.Channel))
			{
				var cache = messages[msg.Channel];

				cache.Messages.Add(msg);

				if (cache.Messages.Count > MessagesLimit)
					cache.Messages.RemoveAt(0);
			}
		}

		public DiscordMessage DeleteMessage(ulong msgId, DiscordChannel textChannel)
		{
			lock (Lock(textChannel))
			{
				var cache = messages[textChannel];

				var message = cache.Messages.Single(s => s.Id == msgId);
				cache.Messages.Remove(message);
				return message;
			}
		}

		public IReadOnlyList<DiscordMessage> GetMessages(DiscordChannel textChannel)
		{
			lock (Lock(textChannel))
			{
				return messages[textChannel].Messages;
			}
		}

		public bool HasMessage(ulong id, DiscordChannel textChannel)
		{
			lock (Lock(textChannel))
			{
				return messages[textChannel].Messages.Any(s => s.Id == id);
			}
		}

		/// <summary>
		/// Deadlock alarm!!!
		/// </summary>
		/// <param name="textChannel"></param>
		/// <returns></returns>
		public Task InitChannelAsync(DiscordChannel textChannel)
		{
			return Task.Run(() =>
			{
				lock (Lock(textChannel))
				{
					messages.TryAdd(textChannel, new());
					var cache = messages[textChannel];

					var msgs = textChannel.GetMessagesAsync(MessagesLimit).Result;
					foreach (var item in msgs)
						AddMessage(item);
				}
			});
		}

		public void DeleteChannelCache(DiscordChannel textChannel)
		{
			lock (Lock(textChannel))
			{
				messages.Remove(textChannel);
			}
		}

		public DiscordMessage GetMessage(ulong id, DiscordChannel textChannel)
		{
			lock (Lock(textChannel))
			{
				return messages[textChannel].Messages.Single(s => s.Id == id);
			}
		}

		public object Lock(DiscordChannel textChannel)
		{
			return messages[textChannel].LockObject;
		}


		private struct CacheItem
		{
			public CacheItem()
			{
				Messages = new();
				LockObject = new();
			}
			

			public List<DiscordMessage> Messages { get; }

			public object LockObject { get; }
		}
	}
}
