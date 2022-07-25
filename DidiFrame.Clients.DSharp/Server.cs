using DidiFrame.Culture;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DidiFrame.Utils;
using DidiFrame.Utils.Collections;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IServer
	/// </summary>
	public class Server : IServer, IDisposable
	{
		private readonly DiscordGuild guild;
		private readonly Client client;
		private readonly Options options;
		private readonly Task globalCacheUpdateTask;
		private readonly CancellationTokenSource cts = new();

		private readonly ThreadsChannelsCollection threadsAndChannels;
		private readonly ChannelCategory globalCategory;
		private readonly ObjectsCache<ulong> serverCache = new();
		private readonly IChannelMessagesCache messages;
		private readonly TextChannelThreadsCache threads;
		private readonly InteractionDispatcherFactory dispatcherFactory;
		private readonly ServerEventsManager events;
		private readonly MessagesEventsManager messagesEvents;


		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? ChannelDeleted
		{ add => events.GetRegistry<IChannel>().AddHandler(value); remove => events.GetRegistry<IChannel>().RemoveHandler(value); }

		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? MemberDeleted
		{ add => events.GetRegistry<IMember>().AddHandler(value); remove => events.GetRegistry<IMember>().RemoveHandler(value); }

		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? RoleDeleted
		{ add => events.GetRegistry<IRole>().AddHandler(value); remove => events.GetRegistry<IRole>().RemoveHandler(value); }

		/// <inheritdoc/>
		public event ServerObjectDeletedEventHandler? CategoryDeleted
		{ add => events.GetRegistry<IChannelCategory>().AddHandler(value); remove => events.GetRegistry<IChannelCategory>().RemoveHandler(value); }

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IChannel>? ChannelCreated
		{ add => events.GetRegistry<IChannel>().AddHandler(value); remove => events.GetRegistry<IChannel>().RemoveHandler(value); }

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IMember>? MemberCreated
		{ add => events.GetRegistry<IMember>().AddHandler(value); remove => events.GetRegistry<IMember>().RemoveHandler(value); }

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IRole>? RoleCreated
		{ add => events.GetRegistry<IRole>().AddHandler(value); remove => events.GetRegistry<IRole>().RemoveHandler(value); }

		/// <inheritdoc/>
		public event ServerObjectCreatedEventHandler<IChannelCategory>? CategoryCreated
		{ add => events.GetRegistry<IChannelCategory>().AddHandler(value); remove => events.GetRegistry<IChannelCategory>().RemoveHandler(value); }

		/// <inheritdoc/>
		public event MessageDeletedEventHandler? MessageDeleted
		{ add => messagesEvents.AddHandler(value); remove => messagesEvents.RemoveHandler(value); }

		/// <inheritdoc/>
		public event MessageSentEventHandler? MessageSent
		{ add => messagesEvents.AddHandler(value); remove => messagesEvents.RemoveHandler(value); }


		/// <inheritdoc/>
		public string Name => AccessBase().Name;

		/// <inheritdoc/>
		public IClient Client => client;

		/// <summary>
		/// Owner client object
		/// </summary>
		public Client SourceClient => client;

		/// <inheritdoc/>
		public ulong Id => AccessBase().Id;

		/// <summary>
		/// Base DiscordGuild from DSharp
		/// </summary>
		public DiscordGuild Guild => AccessBase();

		/// <inheritdoc/>
		public bool IsClosed { get; private set; }


		public void CacheChannel(DiscordChannel channel)
		{
			var frame = serverCache.GetFrame<DiscordChannel>();
			lock (frame)
			{
				if (frame.HasObject(channel.Id))
					frame.DeleteObject(channel.Id);
				frame.AddObject(channel.Id, channel);
			}
		}

		public void CacheMessage(DiscordMessage message)
		{
			lock (messages.Lock(message.Channel))
			{
				if (messages.HasMessage(message.Id, message.Channel))
					messages.DeleteMessage(message.Id, message.Channel);
				messages.AddMessage(message);
			}
		}

		public void AddMessageSentEventHandler(DiscordChannel channel, MessageSentEventHandler? handler) => messagesEvents.AddHandler(channel, handler);

		public void AddMessageDeletedEventHandler(DiscordChannel channel, MessageDeletedEventHandler? handler) => messagesEvents.AddHandler(channel, handler);

		public void RemoveMessageSentEventHandler(DiscordChannel channel, MessageSentEventHandler? handler) => messagesEvents.RemoveHandler(channel, handler);

		public void RemoveMessageDeletedEventHandler(DiscordChannel channel, MessageDeletedEventHandler? handler) => messagesEvents.RemoveHandler(channel, handler);

		public IChannelMessagesCache GetMessagesCache() => messages;

		public IReadOnlyCollection<ITextThread> GetThreadsFor(TextChannel channel)
		{
			var baseChannel = channel.BaseChannel;
			var channelThreads = threads.GetThreadsFor(baseChannel);
			return channelThreads.Select(s =>
			{
				var threadId = s.Id;
				return new TextThread(threadId, channel, () => threads.GetNullableThread(threadId), this, () => (ChannelCategory?)channel.Category);
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

		private DiscordGuild AccessBase([CallerMemberName] string nameOfCaller = "") =>
			IsClosed ? throw new ObjectDoesNotExistException(nameOfCaller) : guild;

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
			client.BaseClient.MessageUpdated -= OnMessageUpdated;
			client.BaseClient.ThreadUpdated -= OnThreadUpdated;

			cts.Cancel();
			globalCacheUpdateTask.Wait();

			IsClosed = true;

			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Creates new isntance of DidiFrame.Clients.DSharp.Server
		/// </summary>
		/// <param name="guild">Base DiscordGuild from DSharp</param>
		/// <param name="client">Owner client object</param>
		public Server(DiscordGuild guild, Client client, Options options, IChannelMessagesCache messagesCache)
		{
			this.guild = guild;
			this.client = client;
			this.options = options;
			globalCategory = new(this);
			dispatcherFactory = new(this);
			messages = messagesCache;
			events = new ServerEventsManager(client.Logger, guild.Id);
			messagesEvents = new(this, client.Logger);


			threads = new(
			(thread) =>
			{
				messages.InitChannelAsync(thread).Wait();
			},

			(thread) =>
			{
				messages.DeleteChannelCache(thread);
			}, options.ThreadCache);

			serverCache.SetRemoveActionHandler<DiscordChannel>((id, channel) =>
			{
				if (channel.IsCategory) return;

				messagesEvents.OnChannelDeleted(channel);

				var type = channel.Type.GetAbstract();
				if (type == Entities.ChannelType.TextCompatible || type == Entities.ChannelType.Voice)
					messages.DeleteChannelCache(channel);

				if (type == Entities.ChannelType.TextCompatible) threads.RemoveAllThreadsThatAssociatedWith(channel);
			});

			serverCache.SetAddActionHandler<DiscordChannel>((id, channel) =>
			{
				if (channel.IsCategory) return;
				var type = channel.Type.GetAbstract();
				if (type == Entities.ChannelType.TextCompatible || type == Entities.ChannelType.Voice)
					messages.InitChannelAsync(channel).Wait();
			});

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
				events.GetRegistry<IChannel>().InvokeCreated(GetChannel(e.ThreadAfter.Id), true);
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
				if (e.ChannelAfter.IsCategory)
					events.GetRegistry<IChannelCategory>().InvokeCreated(GetCategory(e.ChannelAfter.Id), true);
				else events.GetRegistry<IChannel>().InvokeCreated(GetChannel(e.ChannelAfter.Id), true);
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
				events.GetRegistry<IRole>().InvokeCreated(GetRole(e.RoleAfter.Id), true);
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
				events.GetRegistry<IMember>().InvokeCreated(GetMember(e.Member.Id), true);
			}

			return Task.CompletedTask;
		}

		private Task OnMessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			if (e.Message.MessageType != MessageType.Default) return Task.CompletedTask;
			if (e.Message.Author.Id == client.SelfAccount.Id) return Task.CompletedTask;

			lock (messages.Lock(e.Channel))
			{
				var id = e.Message.Id;
				var channel = e.Channel;

				messages.DeleteMessage(id, channel);
				messages.AddMessage(e.Message);
				messagesEvents.InvokeSentEvent(new Message(e.Message.Id, () => messages.GetNullableMessage(id, channel), (TextChannelBase)GetChannel(e.Channel.Id)), true);
			}

			return Task.CompletedTask;
		}

		private Task OnThreadDeleted(DiscordClient sender, ThreadDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			threads.RemoveThread(e.Thread.Id);
			events.GetRegistry<IMember>().InvokeDeleted(this, e.Thread.Id);

			return Task.CompletedTask;
		}

		private Task OnThreadCreated(DiscordClient sender, ThreadCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			if (e.NewlyCreated == false) return Task.CompletedTask;

			threads.AddThread(e.Thread);
			events.GetRegistry<IChannel>().InvokeCreated(GetChannel(e.Thread.Id), true);

			return Task.CompletedTask;
		}

		private Task OnChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.DeleteObject<DiscordChannel>(e.Channel.Id);
			if (e.Channel.IsCategory)
				events.GetRegistry<IChannelCategory>().InvokeDeleted(this, e.Channel.Id);
			else events.GetRegistry<IChannel>().InvokeDeleted(this, e.Channel.Id);

			return Task.CompletedTask;
		}

		private Task OnRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.DeleteObject<DiscordRole>(e.Role.Id);
			events.GetRegistry<IRole>().InvokeDeleted(this, e.Role.Id);

			return Task.CompletedTask;
		}

		private Task OnMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.DeleteObject<DiscordMember>(e.Member.Id);
			events.GetRegistry<IMember>().InvokeDeleted(this, e.Member.Id);

			return Task.CompletedTask;
		}

		private Task OnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			lock (messages.Lock(e.Channel))
			{
				var channel = (TextChannelBase)GetChannel(e.Message.ChannelId);
				messages.DeleteMessage(e.Message.Id, e.Channel);
				dispatcherFactory.DisposeInstance(e.Message.Id);
				messagesEvents.InvokeDeletedEvent((ITextChannelBase)GetChannel(e.Channel.Id), e.Message.Id);
			}
			return Task.CompletedTask;
		}

		private Task OnChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.AddObject(e.Channel.Id, e.Channel);
			if (e.Channel.IsCategory)
				events.GetRegistry<IChannelCategory>().InvokeCreated(GetCategory(e.Channel.Id), false);
			else events.GetRegistry<IChannel>().InvokeCreated(GetChannel(e.Channel.Id), false);

			return Task.CompletedTask;
		}

		private Task OnRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.AddObject(e.Role.Id, e.Role);
			events.GetRegistry<IRole>().InvokeCreated(GetRole(e.Role.Id), false);

			return Task.CompletedTask;
		}

		private Task OnMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.AddObject(e.Member.Id, e.Member);
			events.GetRegistry<IMember>().InvokeCreated(GetMember(e.Member.Id), false);

			return Task.CompletedTask;
		}

		private Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			if (e.Message.MessageType != MessageType.Default) return Task.CompletedTask;
			if (e.Message.Author.Id == client.SelfAccount.Id) return Task.CompletedTask;

			lock (messages.Lock(e.Channel))
			{
				var dchannel = e.Channel;
				var channel = (TextChannelBase)GetChannel(e.Channel.Id);
				messages.AddMessage(e.Message);
				var id = e.Message.Id;
				var messageModel = new Message(id, () => messages.GetMessage(id, channel.BaseChannel), channel);
				messagesEvents.InvokeSentEvent(new Message(e.Message.Id, () => messages.GetNullableMessage(id, dchannel), channel), false);

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
					var guild = AccessBase(nameof(CreateServerCacheUpdateTask));

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
						var newData = guild.GetChannelsAsync().Result;

						var from = newData.ToDictionary(s => s.Id);
						var difference = new CollectionDifference<DiscordChannel, DiscordChannel, ulong>(from.Values.ToArray(), channelsFrame.GetObjects(), s => s.Id, s => s.Id);
						foreach (var item in difference.CalculateDifference())
							if (item.Type == CollectionDifference<DiscordChannel, DiscordChannel, ulong>.OperationType.Add)
							{
								var channel = from[item.Key];
								channelsFrame.AddObject(item.Key, channel);
							}
							else channelsFrame.DeleteObject(item.Key);

						lock (threads)
						{
							var archivedThreads = channelsFrame.GetObjects().SelectMany(s => s.ListPublicArchivedThreadsAsync().Result.Threads.Concat(s.ListPrivateArchivedThreadsAsync().Result.Threads));
							var activeThreads = guild.ListActiveThreadsAsync().Result.Threads;
							var fromT = (options.ThreadCache == Options.ThreadCacheBehavior.CacheAll ? activeThreads.Concat(archivedThreads) : activeThreads).ToDictionary(s => s.Id);
							var differenceT = new CollectionDifference<DiscordThreadChannel, DiscordThreadChannel, ulong>(fromT.Values.ToArray(), threads.GetThreads(), s => s.Id, s => s.Id);
							foreach (var item in differenceT.CalculateDifference())
								if (item.Type == CollectionDifference<DiscordThreadChannel, DiscordThreadChannel, ulong>.OperationType.Add)
								{
									var thread = fromT[item.Key];
									threads.AddThread(thread);
								}
								else threads.RemoveThread(item.Key);
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
						foreach (var item in channels.GetObjects().Where(s => !s.IsCategory))
						{
							var id = item.Id;
							yield return Channel.Construct(id, item.Type.GetAbstract(), () => channels.GetNullableObject(id), server);
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
							return Channel.Construct(id, channels.GetObject(id).Type.GetAbstract(), () => channels.GetNullableObject(id), server);
						else if (threads.HasThread(id))
						{
							var categoryId = threads.GetThread(id).Parent.ParentId;
							var parentId = threads.GetThread(id).Parent.Id;
							return new TextThread(id, (TextChannel)server.GetChannel(parentId), () => threads.GetThread(id), server, () => (ChannelCategory?)server.GetCategory(categoryId));
						}
						else return new NullChannel(id);
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

		private class MessagesEventsManager
		{
			private static readonly EventId SentEventHandlerErrorID = new(11, "SentEventHandlerError");
			private static readonly EventId DeletedEventHandlerErrorID = new(12, "DeletedEventHandlerError");


			private readonly ConcurrentDictionary<ulong, List<MessageDeletedEventHandler>> messageDeletedHandlers = new();
			private readonly ConcurrentDictionary<ulong, List<MessageSentEventHandler>> messageSentHandlers = new();
			private readonly List<MessageDeletedEventHandler> messageDeletedGHandlers = new();
			private readonly List<MessageSentEventHandler> messageSentGHandlers = new();
			private readonly Server owner;
			private readonly ILogger logger;


			public MessagesEventsManager(Server owner, ILogger logger)
			{
				this.owner = owner;
				this.logger = logger;
			}


			public void AddHandler(DiscordChannel channel, MessageDeletedEventHandler? handler)
			{ lock (this) { messageDeletedHandlers.GetOrAdd(channel.Id, _ => new()).Add(handler ?? throw new NullReferenceException()); } }

			public void AddHandler(DiscordChannel channel, MessageSentEventHandler? handler)
			{ lock (this) { messageSentHandlers.GetOrAdd(channel.Id, _ => new()).Add(handler ?? throw new NullReferenceException()); } }

			public void AddHandler(MessageDeletedEventHandler? handler)
			{ lock (this) { messageDeletedGHandlers.Add(handler ?? throw new NullReferenceException()); } }

			public void AddHandler(MessageSentEventHandler? handler)
			{ lock (this) { messageSentGHandlers.Add(handler ?? throw new NullReferenceException()); } }

			public void RemoveHandler(DiscordChannel channel, MessageDeletedEventHandler? handler)
			{ lock (this) { messageDeletedHandlers.GetOrAdd(channel.Id, _ => new()).Remove(handler ?? throw new NullReferenceException()); } }

			public void RemoveHandler(DiscordChannel channel, MessageSentEventHandler? handler)
			{ lock (this) { messageSentHandlers.GetOrAdd(channel.Id, _ => new()).Remove(handler ?? throw new NullReferenceException()); } }

			public void RemoveHandler(MessageDeletedEventHandler? handler)
			{ lock (this) { messageDeletedGHandlers.Remove(handler ?? throw new NullReferenceException()); } }

			public void RemoveHandler(MessageSentEventHandler? handler)
			{ lock (this) { messageSentGHandlers.Remove(handler ?? throw new NullReferenceException()); } }


			public void InvokeSentEvent(IMessage message, bool isModified)
			{
				owner.SourceClient.CultureProvider.SetupCulture(owner);

				foreach (var handler in messageSentGHandlers.Concat(messageSentHandlers.GetOrAdd(message.TextChannel.Id, _ => new())))
				{
					try
					{
						handler.Invoke(owner.Client, message, isModified);
					}
					catch (Exception ex)
					{
						logger.Log(LogLevel.Warning, SentEventHandlerErrorID, ex, "Exception in event handler for message creation ({ModifyStatus}) in {ChannelId} in {ServerId}",
							isModified ? "Modify" : "Create", message.TextChannel.Id, owner.Id);
					}
				}
			}

			public void InvokeDeletedEvent(ITextChannelBase textChannel, ulong messageId)
			{
				owner.SourceClient.CultureProvider.SetupCulture(owner);

				foreach (var handler in messageDeletedGHandlers.Concat(messageDeletedHandlers.GetOrAdd(textChannel.Id, _ => new())))
				{
					try
					{
						handler.Invoke(owner.Client, textChannel, messageId);
					}
					catch (Exception ex)
					{
						logger.Log(LogLevel.Warning, DeletedEventHandlerErrorID, ex, "Exception in event handler for message deleting in {ChannelId} in {ServerId}", textChannel.Id, owner.Id);
					}
				}
			}

			public void OnChannelDeleted(DiscordChannel channel)
			{
				lock (this)
				{
					messageDeletedHandlers.TryRemove(channel.Id, out _);
					messageSentHandlers.TryRemove(channel.Id, out _);
				}
			}
		}

		private class TextChannelThreadsCache
		{
			private readonly Dictionary<ulong, DiscordThreadChannel> threads = new();
			private readonly Action<DiscordThreadChannel> addAction;
			private readonly Action<DiscordThreadChannel> removeAction;
			private readonly Options.ThreadCacheBehavior cacheBehavior;


			public TextChannelThreadsCache(Action<DiscordThreadChannel> addAction, Action<DiscordThreadChannel> removeAction, Options.ThreadCacheBehavior cacheBehavior)
			{
				this.addAction = addAction;
				this.removeAction = removeAction;
				this.cacheBehavior = cacheBehavior;
			}


			public void AddThread(DiscordThreadChannel thread)
			{
				if (cacheBehavior == Options.ThreadCacheBehavior.CacheActive && thread.ThreadMetadata.IsArchived) return;

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
					if (threads[threadId].ThreadMetadata.IsArchived && cacheBehavior == Options.ThreadCacheBehavior.CacheActive) threads.Remove(threadId);
					return threads[threadId];
				}
			}

			public DiscordThreadChannel? GetNullableThread(ulong threadId)
			{
				lock (this)
				{
					return threads.ContainsKey(threadId) ? threads[threadId] : null;
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
						if (item.Parent == channel && (!item.ThreadMetadata.IsArchived || cacheBehavior != Options.ThreadCacheBehavior.CacheActive))
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
			private readonly IReadOnlyCollection<ChannelCategory> categories;


			public int Count => categories.Count;


			public CategoriesCollection(ObjectsCache<ulong>.Frame<DiscordChannel> frame, ChannelCategory globalCategory, Server server)
			{
				categories = frame.GetObjects().Where(s => s.IsCategory).Select(s =>
				{
					var id = s.Id;
					return new ChannelCategory(id, () => frame.GetNullableObject(id), server);
				}).Append(globalCategory).ToArray();
			}


			public IEnumerator<ChannelCategory> GetEnumerator()
			{
				foreach (var item in categories) yield return item;
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		public class Options
		{
			public ThreadCacheBehavior ThreadCache { get; set; } = ThreadCacheBehavior.CacheActive;


			public enum ThreadCacheBehavior
			{
				CacheAll,
				CacheActive
			}
		}
	}
}
