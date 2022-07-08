﻿using DidiFrame.Culture;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DidiFrame.Utils;
using DidiFrame.Utils.Collections;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections;
using System.Threading.Channels;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IServer
	/// </summary>
	public class Server : IServer, IDisposable
	{
		private readonly DiscordGuild guild;
		private readonly Client client;
		private readonly Task globalCacheUpdateTask;
		private readonly CancellationTokenSource cts = new();

		private readonly ThreadsChannelsCollection threadsAndChannels;
		private readonly ChannelCategory globalCategory;
		private readonly ObjectsCache<ulong> serverCache = new();
		private readonly ChannelMessagesCache messages = new();
		private readonly TextChannelThreadsCache threads;
		private readonly InteractionDispatcherFactory dispatcherFactory;


		/// <inheritdoc/>
		public event MessageDeletedEventHandler MessageDeleted
		{
			add => SourceClient.MessageDeleted += new MessageDeletedSubscriber(Id, value).Handle;
			remove => SourceClient.MessageDeleted -= new MessageDeletedSubscriber(Id, value).Handle;
		}

		/// <inheritdoc/>
		public event MessageSentEventHandler MessageSent
		{
			add => SourceClient.MessageSent += new MessageSentSubscriber(Id, value).Handle;
			remove => SourceClient.MessageSent -= new MessageSentSubscriber(Id, value).Handle;
		}


		/// <inheritdoc/>
		public string Name => guild.Name;

		/// <inheritdoc/>
		public IClient Client => client;

		/// <summary>
		/// Owner client object
		/// </summary>
		public Client SourceClient => client;

		/// <inheritdoc/>
		public ulong Id => guild.Id;

		/// <summary>
		/// Base DiscordGuild from DSharp
		/// </summary>
		public DiscordGuild Guild => guild;


		public void CacheMessage(DiscordMessage message)
		{
			lock (messages.Lock(message.Channel))
			{
				if (messages.HasMessage(message.Id, message.Channel))
					messages.DeleteMessage(message.Id, message.Channel);
				messages.AddMessage(message);
			}
		}

		public ChannelMessagesCache GetMessagesCache() => messages;

		public IReadOnlyCollection<ITextThread> GetThreadsFor(TextChannel channel)
		{
			var baseChannel = channel.BaseChannel;
			var channelThreads = threads.GetThreadsFor(baseChannel);
			return channelThreads.Select(s =>
			{
				var threadId = s.Id;
				return new TextThread(threadId, channel, () => threads.GetThread(threadId), this, () => (ChannelCategory?)channel.Category);
			}).ToArray();
		}

		public IInteractionDispatcher GetInteractionDispatcherFor(Message message) => dispatcherFactory.CreateInstance(message);

		public void ResetInteractionDispatcherFor(ulong messageId) => dispatcherFactory.ResetInstance(messageId);

		/// <inheritdoc/>
		public IMember GetMember(ulong id) => new Member(id, () => serverCache.GetFrame<DiscordMember>().GetNullableObject(id), this);

		/// <inheritdoc/>
		public IReadOnlyCollection<IMember> GetMembers() => serverCache.GetFrame<DiscordMember>().GetObjects().Select(s => GetMember(s.Id)).ToArray();

		/// <inheritdoc/>
		public IChannelCategory GetCategory(ulong? id) => id is null ? globalCategory : new ChannelCategory(id.Value, () => serverCache.GetFrame<DiscordChannel>().GetNullableObject(id.Value), this);

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannelCategory> GetCategories() => new CategoriesCollection(serverCache.GetFrame<DiscordChannel>(), globalCategory, this).ToArray();

		/// <inheritdoc/>
		public IChannel GetChannel(ulong id) => threadsAndChannels.GetChannel(id);

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannel> GetChannels() => threadsAndChannels.ToArray();

		/// <inheritdoc/>
		public IRole GetRole(ulong id) => new Role(id, () => serverCache.GetFrame<DiscordRole>().GetNullableObject(id), this);

		/// <inheritdoc/>
		public IReadOnlyCollection<IRole> GetRoles() => serverCache.GetFrame<DiscordRole>().GetObjects().Select(s => GetRole(s.Id)).ToArray();

		/// <inheritdoc/>
		public bool Equals(IServer? other) => other is Server server && server.Id == Id;

		//public bool HasChannel(IChannel channel) => threadsAndChannels.HasChannel(channel.Id);

		//public bool HasMember(IMember member) => serverCache.GetFrame<Member>().HasObject(member.Id);

		//public bool HasRole(IRole member) => serverCache.GetFrame<Role>().HasObject(member.Id);

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as Server);

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();

		/// <inheritdoc/>
		public void Dispose()
		{
			//Detatch events
			client.BaseClient.GuildMemberAdded -= OnMemberAdded;
			client.BaseClient.GuildRoleCreated -= OnRoleCreated;
			client.BaseClient.ChannelCreated -= OnChannelCreated;
			client.BaseClient.MessageCreated -= OnMessageCreated;
			client.BaseClient.ThreadCreated -= OnThreadCreated;

			client.BaseClient.GuildMemberRemoved -= OnMemberRemoved;
			client.BaseClient.GuildRoleDeleted -= OnRoleDeleted;
			client.BaseClient.ChannelDeleted -= OnChannelDeleted;
			client.BaseClient.MessageDeleted -= OnMessageDeleted;
			client.BaseClient.ThreadDeleted -= OnThreadDeleted;

			client.BaseClient.GuildMemberUpdated -= OnMemberUpdated;
			client.BaseClient.GuildRoleUpdated -= OnRoleUpdated;
			client.BaseClient.ChannelUpdated -= OnChannelUpdated;
			client.BaseClient.ThreadUpdated -= OnThreadUpdated;
			client.BaseClient.MessageUpdated -= OnMessageUpdated;

			cts.Cancel();
			globalCacheUpdateTask.Wait();

			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Creates new isntance of DidiFrame.Clients.DSharp.Server
		/// </summary>
		/// <param name="guild">Base DiscordGuild from DSharp</param>
		/// <param name="client">Owner client object</param>
		public Server(DiscordGuild guild, Client client)
		{
			this.guild = guild;
			this.client = client;
			globalCategory = new(this);
			dispatcherFactory = new(this);


			threads = new(
			(thread) =>
			{
				messages.InitChannelAsync(thread).Wait();
			},

			(thread) =>
			{
				messages.DeleteChannelCache(thread);
			});

			serverCache.SetRemoveActionHandler<DiscordChannel>((id, channel) =>
			{
				var type = channel.Type.GetAbstract();
				if (type == Entities.ChannelType.TextCompatible || type == Entities.ChannelType.Voice)
					messages.DeleteChannelCache(channel);

				if (type == Entities.ChannelType.TextCompatible) threads.RemoveAllThreadsThatAssociatedWith(channel);
			});

			serverCache.SetAddActionHandler<DiscordChannel>((id, channel) =>
			{
				var type = channel.Type.GetAbstract();
				if (type == Entities.ChannelType.TextCompatible || type == Entities.ChannelType.Voice)
					messages.InitChannelAsync(channel).Wait();
			});

			messages.MessageDeleted += (msg) =>
			{
				dispatcherFactory.DisposeInstance(msg.Id);
			};

			threadsAndChannels = new(serverCache.GetFrame<DiscordChannel>(), threads, this);


			client.BaseClient.GuildMemberAdded += OnMemberAdded;
			client.BaseClient.GuildRoleCreated += OnRoleCreated;
			client.BaseClient.ChannelCreated += OnChannelCreated;
			client.BaseClient.MessageCreated += OnMessageCreated;
			client.BaseClient.ThreadCreated += OnThreadCreated;

			client.BaseClient.GuildMemberRemoved += OnMemberRemoved;
			client.BaseClient.GuildRoleDeleted += OnRoleDeleted;
			client.BaseClient.ChannelDeleted += OnChannelDeleted;
			client.BaseClient.MessageDeleted += OnMessageDeleted;
			client.BaseClient.ThreadDeleted += OnThreadDeleted;
			
			client.BaseClient.GuildMemberUpdated += OnMemberUpdated;
			client.BaseClient.GuildRoleUpdated += OnRoleUpdated;
			client.BaseClient.ChannelUpdated += OnChannelUpdated;
			client.BaseClient.MessageUpdated += OnMessageUpdated;
			client.BaseClient.ThreadUpdated += OnThreadUpdated;


			globalCacheUpdateTask = CreateServerCacheUpdateTask(cts.Token);
		}


		private Task OnThreadUpdated(DiscordClient sender, ThreadUpdateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			
			lock (threads)
			{
				threads.RemoveThread(e.ThreadAfter.Id);
				threads.AddThread(e.ThreadAfter);
			}

			return Task.CompletedTask;
		}

		private Task OnChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			var frame = serverCache.GetFrame<DiscordChannel>();
			lock (frame)
			{
				frame.DeleteObject(e.ChannelAfter.Id);
				frame.AddObject(e.ChannelAfter.Id, e.ChannelAfter);
			}

			return Task.CompletedTask;
		}

		private Task OnRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			var frame = serverCache.GetFrame<DiscordRole>();
			lock (frame)
			{
				frame.DeleteObject(e.RoleAfter.Id);
				frame.AddObject(e.RoleAfter.Id, e.RoleAfter);
			}

			return Task.CompletedTask;
		}

		private Task OnMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			var frame = serverCache.GetFrame<DiscordMember>();
			lock (frame)
			{
				frame.DeleteObject(e.Member.Id);
				frame.AddObject(e.Member.Id, e.Member);
			}

			return Task.CompletedTask;
		}

		private Task OnMessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			if (e.Message.MessageType != MessageType.Default) return Task.CompletedTask;

			lock (messages.Lock(e.Channel))
			{
				messages.DeleteMessage(e.Message.Id, e.Channel);
				messages.AddMessage(e.Message);
			}

			return Task.CompletedTask;
		}

		private Task OnThreadDeleted(DiscordClient sender, ThreadDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			threads.RemoveThread(e.Thread.Id);

			return Task.CompletedTask;
		}

		private Task OnThreadCreated(DiscordClient sender, ThreadCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			if (e.NewlyCreated == false) return Task.CompletedTask;

			threads.AddThread(e.Thread);

			return Task.CompletedTask;
		}

		private Task OnChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.DeleteObject<DiscordChannel>(e.Channel.Id);

			return Task.CompletedTask;
		}

		private Task OnRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.DeleteObject<DiscordRole>(e.Role.Id);

			return Task.CompletedTask;
		}

		private Task OnMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.DeleteObject<DiscordMember>(e.Member.Id);

			return Task.CompletedTask;
		}

		private Task OnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			if (e.Message.MessageType != MessageType.Default) return Task.CompletedTask;

			lock (messages.Lock(e.Channel))
			{
				var channel = (TextChannelBase)GetChannel(e.Message.ChannelId);
				var deletedMsg = messages.DeleteMessage(e.Message.Id, e.Channel);
				client.CultureProvider.SetupCulture(this);
				SourceClient.OnMessageDeleted(new Message(e.Message.Id, () => deletedMsg, channel));
			}
			return Task.CompletedTask;
		}

		private Task OnChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.AddObject(e.Channel.Id, e.Channel);

			return Task.CompletedTask;
		}

		private Task OnRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.AddObject(e.Role.Id, e.Role);

			return Task.CompletedTask;
		}

		private Task OnMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.AddObject(e.Member.Id, e.Member);

			return Task.CompletedTask;
		}

		private Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			if (e.Message.MessageType != MessageType.Default) return Task.CompletedTask;

			lock (messages.Lock(e.Channel))
			{
				var channel = (TextChannelBase)GetChannel(e.Channel.Id);
				messages.AddMessage(e.Message);
				var id = e.Message.Id;
				var message = new Message(id, () => messages.GetMessage(id, channel.BaseChannel), channel);
				client.CultureProvider.SetupCulture(this);
				SourceClient.OnMessageCreated(message, false);
			}

			return Task.CompletedTask;
		}

		private async Task CreateServerCacheUpdateTask(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				update();
				await Task.Delay(new TimeSpan(0, 5, 0), token);
			}


			void update() => client.DoSafeOperation(() =>
				{
					var memebersFrame = serverCache.GetFrame<DiscordMember>();
					lock (memebersFrame)
					{
						var from = guild.GetAllMembersAsync().Result.ToDictionary(s => s.Id);
						var difference = new CollectionDifference<DiscordMember, DiscordMember, ulong>(from.Values, memebersFrame.GetObjects(), s => s.Id, s => s.Id);
						foreach (var item in difference.CalculateDifference())
							if (item.Type == CollectionDifference<DiscordMember, DiscordMember, ulong>.OperationType.Add)
							{
								var member = from[item.Key];
								memebersFrame.AddObject(item.Key, member);
							}
							else memebersFrame.DeleteObject(item.Key);
					}

					var rolesFrame = serverCache.GetFrame<DiscordRole>();
					lock (rolesFrame)
					{
						var from = guild.Roles;
						var difference = new CollectionDifference<DiscordRole, DiscordRole, ulong>(from.Values.ToArray(), rolesFrame.GetObjects(), s => s.Id, s => s.Id);
						foreach (var item in difference.CalculateDifference())
							if (item.Type == CollectionDifference<DiscordRole, DiscordRole, ulong>.OperationType.Add)
							{
								var role = from[item.Key];
								rolesFrame.AddObject(item.Key, role);
							}
							else rolesFrame.DeleteObject(item.Key);
					}

					var channelsFrame = serverCache.GetFrame<DiscordChannel>();
					lock (channelsFrame)
					{
						lock (threads)
						{
							var newData = guild.GetChannelsAsync().Result;

							var from = newData.ToDictionary(s => s.Id);
							var difference = new CollectionDifference<DiscordChannel, DiscordChannel, ulong>(from.Values.ToArray(), channelsFrame.GetObjects(), s => s.Id, s => s.Id);
							foreach (var item in difference.CalculateDifference())
								if (item.Type == CollectionDifference<DiscordChannel, DiscordChannel, ulong>.OperationType.Add)
								{
									var channel = from[item.Key];
									channelsFrame.AddObject(item.Key, channel);

									//Load threads
									var type = channel.Type.GetAbstract();
									if (type == Entities.ChannelType.TextCompatible)
									{
										var newThreads = channel.Threads.ToDictionary(s => s.Id);
										var threadDiff = new CollectionDifference<DiscordThreadChannel, DiscordThreadChannel, ulong>(newThreads.Values, threads.GetThreadsFor(channel), s => s.Id, s => s.Id);
										foreach (var diff in threadDiff.CalculateDifference())
											if (diff.Type == CollectionDifference<DiscordThreadChannel, DiscordThreadChannel, ulong>.OperationType.Add)
												threads.AddThread(newThreads[diff.Key]);
											else threads.RemoveThread(diff.Key);
									}
								}
								else channelsFrame.DeleteObject(item.Key);
						}
					}
				});
		}


		private class ThreadsChannelsCollection : IReadOnlyCollection<IChannel>
		{
			private readonly ObjectsCache<ulong>.Frame<DiscordChannel> channels;
			private readonly TextChannelThreadsCache threads;
			private readonly Server server;


			public int Count => channels.GetObjects().Count + threads.GetThreads().Count;


			public ThreadsChannelsCollection(ObjectsCache<ulong>.Frame<DiscordChannel> channels, TextChannelThreadsCache threads, Server server)
			{
				this.channels = channels;
				this.threads = threads;
				this.server = server;
			}


			public IEnumerator<IChannel> GetEnumerator()
			{
				lock (channels)
				{
					lock (threads)
					{
						foreach (var item in channels.GetObjects())
						{
							var id = item.Id;
							yield return Channel.Construct(id, () => channels.GetNullableObject(id), server);
						}
						foreach (var item in threads.GetThreads())
						{
							var id = item.Id;
							var parentId = item.Parent.Id;
							yield return new TextThread(id, (TextChannel)server.GetChannel(parentId), () => threads.GetThread(id), server, () => (ChannelCategory?)server.GetChannel(parentId).Category);
						}
					}
				}
			}

			public IChannel GetChannel(ulong id)
			{
				lock (channels)
				{
					lock (threads)
					{
						if (channels.HasObject(id))
							return Channel.Construct(id, () => channels.GetNullableObject(id), server);
						else
						{
							var parentId = threads.GetThread(id).Parent.Id;
							return new TextThread(id, (TextChannel)server.GetChannel(parentId), () => threads.GetThread(id), server, () => (ChannelCategory?)server.GetChannel(parentId).Category);
						}
					}
				}
			}

			public bool HasChannel(ulong id)
			{
				lock (channels)
				{
					lock (threads)
					{
						return channels.HasObject(id) || threads.HasThread(id);
					}
				}
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		private struct MessageDeletedSubscriber
		{
			private readonly ulong serverId;
			private readonly MessageDeletedEventHandler handler;


			public MessageDeletedSubscriber(ulong serverId, MessageDeletedEventHandler handler) : this()
			{
				this.serverId = serverId;
				this.handler = handler;
			}


			public void Handle(IClient client, IMessage message)
			{
				if (message.TextChannel.Server.Id == serverId)
					handler.Invoke(client, message);
			}
		}

		private struct MessageSentSubscriber
		{
			private readonly ulong serverId;
			private readonly MessageSentEventHandler handler;


			public MessageSentSubscriber(ulong serverId, MessageSentEventHandler handler) : this()
			{
				this.serverId = serverId;
				this.handler = handler;
			}


			public void Handle(IClient client, IMessage message, bool isModified)
			{
				if (message.TextChannel.Server.Id == serverId)
					handler.Invoke(client, message, isModified);
			}
		}

		private class TextChannelThreadsCache
		{
			private readonly Dictionary<ulong, DiscordThreadChannel> threads = new();
			private readonly Action<DiscordThreadChannel> addAction;
			private readonly Action<DiscordThreadChannel> removeAction;


			public TextChannelThreadsCache(Action<DiscordThreadChannel> addAction, Action<DiscordThreadChannel> removeAction)
			{
				this.addAction = addAction;
				this.removeAction = removeAction;
			}


			public void AddThread(DiscordThreadChannel thread)
			{
				lock (this)
				{
					threads.Add(thread.Id, thread);
					addAction(thread);
				}
			}

			public void RemoveThread(ulong threadId)
			{
				lock (this)
				{
					var removed = GetThread(threadId);
					threads.Remove(threadId);
					removeAction(removed);
				}
			}

			public DiscordThreadChannel GetThread(ulong threadId)
			{
				lock (this)
				{
					return threads[threadId];
				}
			}

			public IReadOnlyCollection<DiscordThreadChannel> GetThreads()
			{
				lock (this)
				{
					return threads.Values;
				}
			}

			public void RemoveAllThreadsThatAssociatedWith(DiscordChannel channel)
			{
				lock (this)
				{
					var toRemove = new List<ulong>();

					foreach (var item in threads)
						if (item.Value.Parent == channel)
							toRemove.Add(item.Key);

					foreach (var item in toRemove) RemoveThread(item);
				}
			}

			public IReadOnlyCollection<DiscordThreadChannel> GetThreadsFor(DiscordChannel channel)
			{
				lock (this)
				{
					var toRet = new List<DiscordThreadChannel>();

					foreach (var item in threads.Values)
						if (item.Parent == channel)
							toRet.Add(item);

					return toRet;
				}
			}

			public bool HasThread(ulong id)
			{
				lock (this)
				{
					return threads.ContainsKey(id);
				}
			}
		}

		private class CategoriesCollection : IReadOnlyCollection<ChannelCategory>
		{
			private readonly ChannelCategory globalCategory;
			private readonly IReadOnlyCollection<ChannelCategory> categories;


			public int Count => categories.Count + 1;


			public CategoriesCollection(ObjectsCache<ulong>.Frame<DiscordChannel> frame, ChannelCategory globalCategory, Server server)
			{
				this.globalCategory = globalCategory;
				categories = frame.GetObjects().Select(s =>
				{
					var id = s.Id;
					return new ChannelCategory(id, () => frame.GetNullableObject(id), server);
				}).ToArray();
			}


			public IEnumerator<ChannelCategory> GetEnumerator()
			{
				foreach (var item in categories) yield return item;
				yield return globalCategory;
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
