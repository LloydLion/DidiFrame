using DidiFrame.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Client.DSharp.DiscordServer
{
	/// <summary>
	/// Represents default implementation of messages cache for dsharp server
	/// </summary>
	public class ChannelMessagesCache : IChannelMessagesCache
	{
		private readonly Dictionary<DiscordChannel, CacheItem> messages = new();
		private readonly List<DiscordChannel>? cacheEnabled;
		private readonly DSharpClient client;
		private readonly CachePolicy policy;
		private readonly int cacheSize;
		private readonly MessagesPreloadingPolicy preloadingPolicy;
		private readonly int messagesToPreload;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.ChannelMessagesCache
		/// </summary>
		/// <param name="client">DSharp client instance</param>
		/// <param name="options">Option to create cache object</param>
		/// <exception cref="ArgumentOutOfRangeException">If options contains invalid sizes</exception>
		public ChannelMessagesCache(DSharpClient client, Options options)
		{
			if (preloadingPolicy == MessagesPreloadingPolicy.FixedCount)
			{
				if (options.MessagesPreloadCount <= 0)
					throw new ArgumentOutOfRangeException(nameof(options), "Preload messages count must be setted and positive if FixedCount messages preloading policy used");
				if (options.MessagesPreloadCount > options.CacheSize)
					throw new ArgumentOutOfRangeException(nameof(options), "Cannot preload more messages then cache size");
			}

			this.client = client;
			policy = options.CachePolicy;
			cacheSize = options.CacheSize;
			preloadingPolicy = options.PreloadingPolicy;
			messagesToPreload = options.MessagesPreloadCount;
			if (policy == CachePolicy.CacheChannelByRequest || policy == CachePolicy.CacheChannelByRequestWithoutREST) cacheEnabled = new();
		}


		private List<DiscordChannel> CacheEnabled => cacheEnabled ?? throw new InvalidOperationException();


		/// <inheritdoc/>
		public void AddMessage(DiscordMessage msg)
		{
			lock (Lock(msg.Channel))
			{
				switch (policy)
				{
					case CachePolicy.Disable or CachePolicy.CacheMessageByRequest:
						return;
					case CachePolicy.CacheAll or CachePolicy.CacheChannelByRequestWithoutREST or CachePolicy.CacheAllWithoutREST or CachePolicy.CacheChannelByRequest:
						{
							if ((policy == CachePolicy.CacheChannelByRequest || policy == CachePolicy.CacheChannelByRequestWithoutREST)
								&& CacheEnabled.Contains(msg.Channel) == false) return;

							var cache = messages[msg.Channel];

							if (cache.Messages.ContainsKey(msg.Id)) return;

							cache.Messages.Add(msg);

							if (cache.Messages.Count > cacheSize)
								cache.Messages.RemoveAt(0);
						}
						break;
					default:
						throw new ImpossibleVariantException();
				}
			}
		}

		/// <inheritdoc/>
		public void DeleteMessage(ulong msgId, DiscordChannel textChannel)
		{
			lock (Lock(textChannel))
			{
				switch (policy)
				{
					case CachePolicy.Disable:
						{
							//Do nothing
						}
						break;
					case CachePolicy.CacheAll or CachePolicy.CacheChannelByRequest or CachePolicy.CacheAllWithoutREST or CachePolicy.CacheChannelByRequestWithoutREST:
						{
							if ((policy == CachePolicy.CacheChannelByRequest || policy == CachePolicy.CacheChannelByRequestWithoutREST)
								&& CacheEnabled.Contains(textChannel) == false) return;

							var cache = messages[textChannel];
							if (cache.Messages.ContainsKey(msgId) == false) return;
							else
							{
								var message = GetMessage(msgId, textChannel);
								cache.Messages.Remove(message);
							}
						}
						break;
					default:
						throw new ImpossibleVariantException();
				}
			}
		}

		/// <inheritdoc/>
		public IReadOnlyList<DiscordMessage> GetMessages(DiscordChannel textChannel, int quantity)
		{
			lock (Lock(textChannel))
			{
				switch (policy)
				{
					case CachePolicy.Disable or CachePolicy.CacheMessageByRequest:
						return client.DoSafeOperation(() => textChannel.GetMessagesAsync(quantity).Result.Reverse().ToArray());
					case CachePolicy.CacheAll or CachePolicy.CacheAllWithoutREST:
						return messages[textChannel].Messages.AsEnumerable().Take(quantity).ToArray();
					case CachePolicy.CacheChannelByRequest or CachePolicy.CacheChannelByRequestWithoutREST:
						if (CacheEnabled.Contains(textChannel)) return messages[textChannel].Messages.AsEnumerable().Take(quantity).ToArray();
						else
						{
							CacheEnabled.Add(textChannel);
							PreloadMessages(textChannel);
							var result = client.DoSafeOperation(() => textChannel.GetMessagesAsync(quantity).Result);
							foreach (var item in result.Reverse())
								AddMessage(item);
							return result;
						}
					default:
						throw new ImpossibleVariantException();
				}
			}
		}

		/// <inheritdoc/>
		public DiscordMessage? GetNullableMessage(ulong id, DiscordChannel textChannel)
		{
			return HasMessage(id, textChannel) ? GetMessage(id, textChannel) : null;
		}

		/// <inheritdoc/>
		public bool HasMessage(ulong id, DiscordChannel textChannel)
		{
			lock (Lock(textChannel))
			{
				switch (policy)
				{
					case CachePolicy.Disable:
						return checkRESTExistance();
					case CachePolicy.CacheAll or CachePolicy.CacheMessageByRequest:
						if (messages[textChannel].Messages.ContainsKey(id)) return true;
						else return checkRESTExistance();
					case CachePolicy.CacheChannelByRequest:
						if (CacheEnabled.Contains(textChannel) == false)
						{
							CacheEnabled.Add(textChannel);
							PreloadMessages(textChannel);
						}

						if (messages[textChannel].Messages.ContainsKey(id)) return true;
						else return checkRESTExistance();
					case CachePolicy.CacheAllWithoutREST:
						return messages[textChannel].Messages.ContainsKey(id);
					case CachePolicy.CacheChannelByRequestWithoutREST:
						if (CacheEnabled.Contains(textChannel) == false)
						{
							CacheEnabled.Add(textChannel);
							PreloadMessages(textChannel);
						}

						return messages[textChannel].Messages.ContainsKey(id);
					default:
						throw new ImpossibleVariantException();
				}
			}

			bool checkRESTExistance()
			{
				return client.DoSafeOperation(() =>
				{
					try
					{
						textChannel.GetMessageAsync(id, true).Wait();
						return true;
					}
					catch (AggregateException aex)
					{
						if (aex.InnerExceptions.Count == 1 && aex.InnerExceptions.Single() is NotFoundException)
							return false;
						else throw;
					}
				});
			}
		}

		/// <inheritdoc/>
		public Task InitChannelAsync(DiscordChannel textChannel)
		{
			switch (policy)
			{
				case CachePolicy.Disable:
					return Task.CompletedTask;
				case CachePolicy.CacheAll or CachePolicy.CacheAllWithoutREST:
					return Task.Run(() =>
					{
						lock (Lock(textChannel))
						{
							PreloadMessages(textChannel);
						}
					});
				case CachePolicy.CacheChannelByRequest or CachePolicy.CacheChannelByRequestWithoutREST or CachePolicy.CacheMessageByRequest:
					lock (Lock(textChannel))
					{
						//Lock will automaticly add cache
						if (messages.ContainsKey(textChannel) == false) messages.Add(textChannel, new());
					}
					return Task.CompletedTask;
				default:
					throw new ImpossibleVariantException();
			}
		}

		/// <inheritdoc/>
		public void DeleteChannelCache(DiscordChannel textChannel)
		{
			lock (Lock(textChannel))
			{
				messages.Remove(textChannel);
			}
		}

		/// <inheritdoc/>
		public DiscordMessage GetMessage(ulong id, DiscordChannel textChannel)
		{
			lock (Lock(textChannel))
			{
				switch (policy)
				{
					case CachePolicy.Disable:
						return textChannel.GetMessageAsync(id, true).Result;
					case CachePolicy.CacheAll or CachePolicy.CacheMessageByRequest:
						{
							var cache = messages[textChannel].Messages;
							if (cache.ContainsKey(id)) return cache[id];
							else
							{
								var request = sendRESTRequest();
								if (policy == CachePolicy.CacheMessageByRequest)
									messages[textChannel].Messages.Add(request);
								return request;
							}
						}
					case CachePolicy.CacheChannelByRequest:
						{
							if (CacheEnabled.Contains(textChannel) == false)
							{
								CacheEnabled.Add(textChannel);
								PreloadMessages(textChannel);
							}

							var cache = messages[textChannel].Messages;
							if (cache.ContainsKey(id)) return cache[id];
							else return sendRESTRequest();
						}
					case CachePolicy.CacheAllWithoutREST:
						{
							return messages[textChannel].Messages[id];
						}
					case CachePolicy.CacheChannelByRequestWithoutREST:
						{
							if (CacheEnabled.Contains(textChannel) == false)
							{
								CacheEnabled.Add(textChannel);
								PreloadMessages(textChannel);
							}

							return messages[textChannel].Messages[id];
						}
					default:
						throw new ImpossibleVariantException();
				}
			}

			DiscordMessage sendRESTRequest()
			{
				return client.DoSafeOperation(() => textChannel.GetMessageAsync(id, true).Result, new(DSharpClient.MessageName, id));
			}
		}

		/// <inheritdoc/>
		public object Lock(DiscordChannel textChannel)
		{
			if (messages.ContainsKey(textChannel) == false) messages.Add(textChannel, new());
			return messages[textChannel].LockObject;
		}

		private void PreloadMessages(DiscordChannel textChannel)
		{
			switch (preloadingPolicy)
			{
				case MessagesPreloadingPolicy.Disable:
					break;
				case MessagesPreloadingPolicy.CacheFull:
					preloadQ(cacheSize);
					break;
				case MessagesPreloadingPolicy.FixedCount:
					preloadQ(messagesToPreload);
					break;
				default:
					throw new ImpossibleVariantException();
			}

			void preloadQ(int quantity)
			{
				if (messages.ContainsKey(textChannel) == false) messages.Add(textChannel, new());
				var msgs = textChannel.GetMessagesAsync(quantity).Result.Where(s => s.MessageType == DSharpPlus.MessageType.Default).Reverse();
				foreach (var item in msgs) messages[textChannel].Messages.Add(item);
			}
		}


		private struct CacheItem
		{
			public CacheItem()
			{
				Messages = new();
				LockObject = new();
			}


			public MessagesCache Messages { get; }

			public object LockObject { get; }
		}

		/// <summary>
		/// Represents messages caching policy
		/// </summary>
		public enum CachePolicy
		{
			/// <summary>
			/// Send REST request for each message
			/// </summary>
			Disable,
			/// <summary>
			/// Enable caching only for requested messages
			/// </summary>
			CacheMessageByRequest,
			/// <summary>
			/// Enable caching for all messages and channels
			/// </summary>
			CacheAll,
			/// <summary>
			/// Enable caching only for channels where message has been requested (it triggers messages preloading).
			/// </summary>
			CacheChannelByRequest,
			/// <summary>
			/// Enable caching for all messages and channels, but message out of cache is unavailable
			/// </summary>
			CacheAllWithoutREST,
			/// <summary>
			/// Enable caching only for channels where message has been requested (it triggers messages preloading),
			/// but message out of cache is unavailable. First message will be taken by REST and loading process will be started
			/// </summary>
			CacheChannelByRequestWithoutREST,
		}

		/// <summary>
		/// Represents messages preloading policy
		/// </summary>
		public enum MessagesPreloadingPolicy
		{
			/// <summary>
			/// Disable any messages preloading (fast bot start, less memory usage, but be careful with WithoutREST cache policies)
			/// </summary>
			Disable,
			/// <summary>
			/// Cache full size of messages that can be stolen in cache when bot starts or when channel starts caching
			/// </summary>
			CacheFull,
			/// <summary>
			/// Cache determined count of messages when bot starts or when channel starts caching
			/// </summary>
			FixedCount
		}

		/// <summary>
		/// Options for DidiFrame.Clients.DSharp.ChannelMessagesCache
		/// </summary>
		public class Options
		{
			/// <summary>
			/// Messages preloading policy
			/// </summary>
			public MessagesPreloadingPolicy PreloadingPolicy { get; set; } = MessagesPreloadingPolicy.CacheFull;

			/// <summary>
			/// Quantity to preload
			/// </summary>
			public int MessagesPreloadCount { get; set; } = -1;

			/// <summary>
			/// Messages cache policy
			/// </summary>
			public CachePolicy CachePolicy { get; set; } = CachePolicy.CacheChannelByRequestWithoutREST;

			/// <summary>
			/// Size of cache
			/// </summary>
			public int CacheSize { get; set; } = 25;
		}

		/// <summary>
		/// Implementation of DidiFrame.Clients.DSharp.IChannelMessagesCacheFactory for DidiFrame.Clients.DSharp.ChannelMessagesCache
		/// </summary>
		public class Factory : IChannelMessagesCacheFactory
		{
			private readonly Options options;


			/// <summary>
			/// Creates new instance of DidiFrame.Clients.DSharp.ChannelMessagesCache.Factory
			/// </summary>
			/// <param name="options">Options for DidiFrame.Clients.DSharp.ChannelMessagesCache</param>
			public Factory(Options options)
			{
				this.options = options;
			}


			/// <inheritdoc/>
			public IChannelMessagesCache Create(DiscordGuild server, DSharpClient client)
			{
				return new ChannelMessagesCache(client, options);
			}
		}

		private sealed class MessagesCache : IReadOnlyDictionary<ulong, DiscordMessage>, IList<DiscordMessage>
		{
			private readonly List<DiscordMessage> orderedMessages = new();
			private readonly Dictionary<ulong, DiscordMessage> keyPairs = new();


			public DiscordMessage this[int index]
			{
				get => orderedMessages[index];
				set
				{
					var res = orderedMessages[index];
					keyPairs[res.Id] = value;
					orderedMessages[index] = value;
				}
			}

			public DiscordMessage this[ulong key] => keyPairs[key];


			public int Count => orderedMessages.Count;

			public bool IsReadOnly => false;

			public IEnumerable<ulong> Keys => keyPairs.Keys;

			public IEnumerable<DiscordMessage> Values => keyPairs.Values;


			public void Add(DiscordMessage item)
			{
				orderedMessages.Add(item);
				keyPairs.Add(item.Id, item);
			}

			public void Clear()
			{
				keyPairs.Clear();
				orderedMessages.Clear();
			}

			public bool Contains(DiscordMessage item)
			{
				return orderedMessages.Contains(item);
			}

			public bool ContainsKey(ulong key)
			{
				return keyPairs.ContainsKey(key);
			}

			public void CopyTo(DiscordMessage[] array, int arrayIndex)
			{
				orderedMessages.CopyTo(array, arrayIndex);
			}

			public IEnumerator<DiscordMessage> GetEnumerator()
			{
				return orderedMessages.GetEnumerator();
			}

			public int IndexOf(DiscordMessage item)
			{
				return orderedMessages.IndexOf(item);
			}

			public void Insert(int index, DiscordMessage item)
			{
				keyPairs.Add(item.Id, item);
				orderedMessages.Insert(index, item);
			}

			public bool Remove(DiscordMessage item)
			{
				keyPairs.Remove(item.Id);
				return orderedMessages.Remove(item);
			}

			public void RemoveAt(int index)
			{
				keyPairs.Remove(orderedMessages[index].Id);
				orderedMessages.RemoveAt(index);
			}

			public bool TryGetValue(ulong key, [MaybeNullWhen(false)] out DiscordMessage value)
			{
				return keyPairs.TryGetValue(key, out value);
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator<KeyValuePair<ulong, DiscordMessage>> IEnumerable<KeyValuePair<ulong, DiscordMessage>>.GetEnumerator()
			{
				return keyPairs.GetEnumerator();
			}

			public IEnumerable<DiscordMessage> AsEnumerable() => this;
		}
	}
}
