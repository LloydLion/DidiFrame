using DidiFrame.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace DidiFrame.Clients.DSharp
{
	public class ChannelMessagesCache
	{
		private readonly Dictionary<DiscordChannel, CacheItem> messages = new();
		private readonly List<DiscordChannel>? cacheEnabled;
		private readonly Server server;
		private readonly CachePolicy policy;
		private readonly int messagesLimit;


		public event Action<DiscordMessage>? MessageDeleted;


		public ChannelMessagesCache(Server server, CachePolicy policy, int messagesLimit)
		{
			this.server = server;
			this.policy = policy;
			this.messagesLimit = messagesLimit;
			if (policy == CachePolicy.EnableByRequest || policy == CachePolicy.EnableByRequestWithoutREST) cacheEnabled = new();
		}


		private List<DiscordChannel> CacheEnabled => cacheEnabled ?? throw new InvalidOperationException();


		public void AddMessage(DiscordMessage msg)
		{
			lock (Lock(msg.Channel))
			{
				switch (policy)
				{
					case CachePolicy.Disable:
						return;
					case CachePolicy.EnableForAll or CachePolicy.EnableByRequestWithoutREST or CachePolicy.EnableForAllWithoutREST or CachePolicy.EnableByRequest:
						{
							if ((policy == CachePolicy.EnableByRequest || policy == CachePolicy.EnableByRequestWithoutREST) && CacheEnabled.Contains(msg.Channel) == false) return;

							var cache = messages[msg.Channel];
							cache.Messages.Add(msg);

							if (cache.Messages.Count > messagesLimit)
							{
								var last = cache.Messages[0];
								cache.Messages.RemoveAt(0);
								MessageDeleted?.Invoke(last);
							}
						}
						break;
					default:
						throw new ImpossibleVariantException();
				}
			}
		}

		public void DeleteMessage(ulong msgId, DiscordChannel textChannel)
		{
			lock (Lock(textChannel))
			{
				switch (policy)
				{
					case CachePolicy.Disable:
						{
							var message = GetMessage(msgId, textChannel);
							MessageDeleted?.Invoke(message);
						}
						break;
					case CachePolicy.EnableForAll or CachePolicy.EnableByRequest or CachePolicy.EnableForAllWithoutREST or CachePolicy.EnableByRequestWithoutREST:
						{
							if ((policy == CachePolicy.EnableByRequest || policy == CachePolicy.EnableByRequestWithoutREST) && CacheEnabled.Contains(textChannel) == false) return;

							var cache = messages[textChannel];
							var message = GetMessage(msgId, textChannel);
							cache.Messages.Remove(message);
							MessageDeleted?.Invoke(message);
						}
						break;
					default:
						throw new ImpossibleVariantException();
				}
			}
		}

		public IReadOnlyList<DiscordMessage> GetMessages(DiscordChannel textChannel, int quantity)
		{
			lock (Lock(textChannel))
			{
				switch (policy)
				{
					case CachePolicy.Disable:
						return server.SourceClient.DoSafeOperation(() => textChannel.GetMessagesAsync(quantity).Result,
							new(Client.ChannelName, textChannel.Id, textChannel.Name));
					case CachePolicy.EnableForAll or CachePolicy.EnableForAllWithoutREST:
						return messages[textChannel].Messages.Take(quantity).ToArray();
					case CachePolicy.EnableByRequest or CachePolicy.EnableByRequestWithoutREST:
						if (CacheEnabled.Contains(textChannel)) return messages[textChannel].Messages.Take(quantity).ToArray();
						else
						{
							CacheEnabled.Add(textChannel);
							var result = server.SourceClient.DoSafeOperation(() => textChannel.GetMessagesAsync(quantity).Result,
								new(Client.ChannelName, textChannel.Id, textChannel.Name));
							foreach (var item in result.Reverse())
								AddMessage(item);
							return result;
						}
					default:
						throw new ImpossibleVariantException();
				}
			}
		}

		public DiscordMessage? GetNullableMessage(ulong id, DiscordChannel textChannel)
		{
			return HasMessage(id, textChannel) ? GetMessage(id, textChannel) : null;
		}

		public bool HasMessage(ulong id, DiscordChannel textChannel)
		{

			lock (Lock(textChannel))
			{
				switch (policy)
				{
					case CachePolicy.Disable:
						return server.SourceClient.DoSafeOperation(() =>
						{
							try
							{
								textChannel.GetMessageAsync(id, true).Wait();
								return true;
							}
							catch (NotFoundException) { return false; }
						});
					case CachePolicy.EnableForAll:
						if (messages[textChannel].Messages.Any(s => s.Id == id)) return true;
						else return server.SourceClient.DoSafeOperation(() =>
						{
							try
							{
								textChannel.GetMessageAsync(id, true).Wait();
								return true;
							}
							catch (NotFoundException) { return false; }
						});
					case CachePolicy.EnableByRequest:
						if (CacheEnabled.Contains(textChannel))
						{
							if (messages[textChannel].Messages.Any(s => s.Id == id)) return true;
							else return server.SourceClient.DoSafeOperation(() =>
							{
								try
								{
									textChannel.GetMessageAsync(id, true).Wait();
									return true;
								}
								catch (NotFoundException) { return false; }
							});
						}
						else
						{
							CacheEnabled.Add(textChannel);
							try
							{
								textChannel.GetMessageAsync(id, true).Wait();
								return true;
							}
							catch (NotFoundException) { return false; }
						}
					case CachePolicy.EnableForAllWithoutREST:
						return messages[textChannel].Messages.Any(s => s.Id == id);
					case CachePolicy.EnableByRequestWithoutREST:
						if (CacheEnabled.Contains(textChannel) == false) CacheEnabled.Add(textChannel);
						return messages[textChannel].Messages.Any(s => s.Id == id);
					default:
						throw new ImpossibleVariantException();
				}
			}
		}

		/// <summary>
		/// Deadlock alarm!!!
		/// </summary>
		/// <param name="textChannel"></param>
		/// <returns></returns>
		public Task InitChannelAsync(DiscordChannel textChannel)
		{
			switch (policy)
			{
				case CachePolicy.Disable:
					return Task.CompletedTask;
				case CachePolicy.EnableForAll or CachePolicy.EnableForAllWithoutREST:
					return Task.Run(() =>
					{
						lock (Lock(textChannel))
						{
							if (messages.ContainsKey(textChannel) == false) messages.Add(textChannel, new());
							var cache = messages[textChannel];

							var msgs = textChannel.GetMessagesAsync(messagesLimit).Result.Where(s => s.MessageType == DSharpPlus.MessageType.Default);
							foreach (var item in msgs.Reverse())
								AddMessage(item);
						}
					});
				case CachePolicy.EnableByRequest or CachePolicy.EnableByRequestWithoutREST:
					lock (Lock(textChannel))
					{
						//Lock will automaticly add cache
						//if (messages.ContainsKey(textChannel) == false) messages.Add(textChannel, new());
					}
					return Task.CompletedTask;
				default:
					throw new ImpossibleVariantException();
			}
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
				switch (policy)
				{
					case CachePolicy.Disable:
						return textChannel.GetMessageAsync(id, true).Result;
					case CachePolicy.EnableForAll:
						{
							var msg = messages[textChannel].Messages.SingleOrDefault(s => s.Id == id);
							return msg ?? textChannel.GetMessageAsync(id, true).Result;
						}
					case CachePolicy.EnableByRequest:
						{
							if (CacheEnabled.Contains(textChannel) == false)
							{
								CacheEnabled.Add(textChannel);
								return textChannel.GetMessageAsync(id, true).Result;
							}
							else
							{
								var msg = messages[textChannel].Messages.SingleOrDefault(s => s.Id == id);
								return msg ?? textChannel.GetMessageAsync(id, true).Result;
							}
						}
					case CachePolicy.EnableForAllWithoutREST:
						{
							return messages[textChannel].Messages.Single(s => s.Id == id);
						}
					case CachePolicy.EnableByRequestWithoutREST:
						{
							if (CacheEnabled.Contains(textChannel) == false)
							{
								CacheEnabled.Add(textChannel);
								return textChannel.GetMessageAsync(id, true).Result;
							}
							else return messages[textChannel].Messages.Single(s => s.Id == id);
						}
					default:
						throw new ImpossibleVariantException();
				}
			}
		}

		public object Lock(DiscordChannel textChannel)
		{
			if (messages.ContainsKey(textChannel) == false) messages.Add(textChannel, new());
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

		public enum CachePolicy
		{
			/// <summary>
			/// Send REST request for each message
			/// </summary>
			Disable,
			/// <summary>
			/// Enable caching for all messages and channels
			/// </summary>
			EnableForAll,
			/// <summary>
			/// Enable caching only for channels where message has been requested
			/// </summary>
			EnableByRequest,
			/// <summary>
			/// Enable caching for all messages and channels, but message out of cache is unavailable
			/// </summary>
			EnableForAllWithoutREST,
			/// <summary>
			/// Enable caching only for channels where message has been requested,
			/// but message out of cache is unavailable. First message will be taken by REST and loading process will be started
			/// </summary>
			EnableByRequestWithoutREST,
		}
	}
}
