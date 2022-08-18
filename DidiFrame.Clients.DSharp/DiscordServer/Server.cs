using DidiFrame.Culture;
using DidiFrame.Exceptions;
using DidiFrame.Client;
using DidiFrame.Utils;
using DidiFrame.Utils.Collections;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DChannelType = DidiFrame.Entities.ChannelType;

namespace DidiFrame.Client.DSharp.DiscordServer
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IServer
	/// </summary>
	public sealed class Server : IServer, IDisposable
	{
		private readonly DiscordGuild guild;
		private readonly Client client;
		private readonly Options options;
		private readonly Task globalCacheUpdateTask;
		private readonly CancellationTokenSource cts = new();

		private readonly ServerChannelsStore channels;
		private readonly ObjectsCache<ulong> serverCache = new();
		private readonly IChannelMessagesCache messages;
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
		public DiscordGuild Guild => AccessBase();

		/// <inheritdoc/>
		public bool IsClosed { get; private set; }


		/// <summary>
		/// Safely adds new channel to cache, or modify it if channel already in cache
		/// </summary>
		/// <param name="channel">Target channel</param>
		public void CacheChannel(DiscordChannel channel)
		{
			lock (channels.SyncRoot)
			{
				channels.UpdateChannel(channel);
			}
		}

		/// <summary>
		/// Safely adds new message to cache, or modify it if message already in cache
		/// </summary>
		/// <param name="message">Target message</param>
		public void CacheMessage(DiscordMessage message)
		{
			lock (messages.Lock(message.Channel))
			{
				if (messages.HasMessage(message.Id, message.Channel))
					messages.DeleteMessage(message.Id, message.Channel);
				messages.AddMessage(message);
			}
		}

		internal void AddMessageSentEventHandler(DiscordChannel channel, MessageSentEventHandler? handler) => messagesEvents.AddHandler(channel, handler);

		internal void AddMessageDeletedEventHandler(DiscordChannel channel, MessageDeletedEventHandler? handler) => messagesEvents.AddHandler(channel, handler);

		internal void RemoveMessageSentEventHandler(DiscordChannel channel, MessageSentEventHandler? handler) => messagesEvents.RemoveHandler(channel, handler);

		internal void RemoveMessageDeletedEventHandler(DiscordChannel channel, MessageDeletedEventHandler? handler) => messagesEvents.RemoveHandler(channel, handler);

		internal IChannelMessagesCache GetMessagesCache() => messages;

		internal IReadOnlyCollection<ITextThread> GetThreadsFor(TextChannel channel) => channels.GetThreadsFor(channel.Id);

		internal IInteractionDispatcher GetInteractionDispatcherFor(Message message) => dispatcherFactory.CreateInstance(message);

		internal void ResetInteractionDispatcherFor(ulong messageId, DiscordChannel channel) => dispatcherFactory.ResetInstance(messageId, channel);

		/// <inheritdoc/>
		public IMember GetMember(ulong id) => new Member(id, () => serverCache.GetFrame<DiscordMember>().GetNullableObject(id), this);

		/// <inheritdoc/>
		public IReadOnlyCollection<IMember> GetMembers() => serverCache.GetFrame<DiscordMember>().GetObjects().Select(s => GetMember(s.Id)).ToArray();

		/// <inheritdoc/>
		public IChannelCategory GetCategory(ulong? id) => channels.GetCategory(id);

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannelCategory> GetCategories() => channels.GetCategories();

		/// <inheritdoc/>
		public IChannel GetChannel(ulong id) => channels.GetChannel(id);

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannel> GetChannels() => channels.GetChannels();

		/// <inheritdoc/>
		public IRole GetRole(ulong id) => new Role(id, () => serverCache.GetFrame<DiscordRole>().GetNullableObject(id), this);

		/// <inheritdoc/>
		public IReadOnlyCollection<IRole> GetRoles() => serverCache.GetFrame<DiscordRole>().GetObjects().Select(s => GetRole(s.Id)).ToArray();

		/// <inheritdoc/>
		public bool Equals(IServer? other) => other is Server server && server.Id == Id;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as Server);

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();

		private DiscordGuild AccessBase([CallerMemberName] string nameOfCaller = "") =>
			IsClosed ? throw new ObjectDoesNotExistException(nameOfCaller) : guild;

		private T ThrowIfClosed<T>(Func<T> getter, [CallerMemberName] string nameOfCaller = "")
		{
			if (IsClosed) throw new ObjectDoesNotExistException(nameOfCaller);
			else return getter();
		}

		private void ThrowIfClosed([CallerMemberName] string nameOfCaller = "")
		{
			if (IsClosed)
				throw new ObjectDoesNotExistException(nameOfCaller);
		}

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
			client.BaseClient.ThreadUpdated -= options.ThreadCache == Options.ThreadCacheBehavior.CacheAll ? OnThreadUpdatedForCacheAllMode : OnThreadUpdatedForCacheActiveMode;

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
		/// <param name="options">Options for server</param>
		/// <param name="messagesCache">channel message cache instance</param>
		private Server(DiscordGuild guild, Client client, Options options, IChannelMessagesCache messagesCache)
		{
			this.guild = guild;
			this.client = client;
			this.options = options;
			dispatcherFactory = new(this);
			messages = messagesCache;
			events = new ServerEventsManager(client.Logger, this);
			messagesEvents = new(client.Logger, this);

			channels = new(this);

			channels.ChannelCreated += (channel, isModified) =>
			{
				if (channel.IsCategory)
					events.GetRegistry<IChannelCategory>().InvokeCreated(channels.GetCategory(channel.Id), isModified);
				else events.GetRegistry<IChannel>().InvokeCreated(channels.GetChannel(channel.Id), isModified);

				var type = channel.Type.GetAbstract();
				var isTextType = type == DChannelType.TextCompatible || type == DChannelType.Voice;
				if (!isModified && !channel.IsCategory && isTextType) messages.InitChannelAsync(channel).Wait();
			};

			channels.ChannelDeleted += (channel) =>
			{
				if (channel.IsCategory)
					events.GetRegistry<IChannelCategory>().InvokeDeleted(channel.Id);
				else events.GetRegistry<IChannel>().InvokeDeleted(channel.Id);

				var type = channel.Type.GetAbstract();
				var isTextType = type == DChannelType.TextCompatible || type == DChannelType.Voice;
				if (!channel.IsCategory && isTextType)
				{
					messages.DeleteChannelCache(channel);
					dispatcherFactory.ClearSubscribers(channel);
					messagesEvents.OnChannelDeleted(channel);
				}
			};

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
			client.BaseClient.ThreadUpdated += options.ThreadCache == Options.ThreadCacheBehavior.CacheAll ? OnThreadUpdatedForCacheAllMode : OnThreadUpdatedForCacheActiveMode;


			globalCacheUpdateTask = CreateServerCacheUpdateTask(cts.Token);
		}


		internal static async Task<Server> CreateServerAsync(DiscordGuild guild, Client client, Options options, IChannelMessagesCache messagesCache)
		{
			var server = new Server(guild, client, options, messagesCache);

			await server.UpdateServerCache();

			return server;
		}

		[SuppressMessage("Major Code Smell", "S1172")]
		private Task OnThreadUpdatedForCacheActiveMode(DiscordClient sender, ThreadUpdateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			if (e.ThreadAfter.ThreadMetadata.IsArchived)
				channels.DeleteChannel(e.ThreadAfter.Id); //Archived case
			else channels.UpdateChannel(e.ThreadAfter); //Other change case or resumed case

			return Task.CompletedTask;
		}

		[SuppressMessage("Major Code Smell", "S1172")]
		private Task OnThreadUpdatedForCacheAllMode(DiscordClient sender, ThreadUpdateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			channels.UpdateChannel(e.ThreadAfter);
			return Task.CompletedTask;
		}

		private Task OnChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			channels.UpdateChannel(e.ChannelAfter);
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
			channels.DeleteChannel(e.Thread.Id);
			return Task.CompletedTask;
		}

		private Task OnThreadCreated(DiscordClient sender, ThreadCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			channels.UpdateChannel(e.Thread);
			return Task.CompletedTask;
		}

		private Task OnChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			channels.DeleteChannel(e.Channel.Id);
			return Task.CompletedTask;
		}

		private Task OnRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.DeleteObject<DiscordRole>(e.Role.Id);
			events.GetRegistry<IRole>().InvokeDeleted(e.Role.Id);

			return Task.CompletedTask;
		}

		private Task OnMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.DeleteObject<DiscordMember>(e.Member.Id);
			events.GetRegistry<IMember>().InvokeDeleted(e.Member.Id);

			return Task.CompletedTask;
		}

		private Task OnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			lock (messages.Lock(e.Channel))
			{
				messages.DeleteMessage(e.Message.Id, e.Channel);
				dispatcherFactory.DisposeInstance(e.Message.Id, e.Channel);
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
				messagesEvents.InvokeSentEvent(new Message(e.Message.Id, () => messages.GetNullableMessage(id, dchannel), channel), false);
			}

			return Task.CompletedTask;
		}

		private async Task CreateServerCacheUpdateTask(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				await Task.Delay(new TimeSpan(0, 5, 0), token);

				try
				{
					await UpdateServerCache();
				}
				catch (Exception)
				{
					//Cache update can throw exception
				}
			}
		}

		private Task UpdateServerCache() => Task.Run(() =>
		{
			client.DoSafeOperation(() =>
			{
				var currentGuild = AccessBase(nameof(UpdateServerCache));

				var membersFrame = serverCache.GetFrame<DiscordMember>();
				lock (membersFrame)
				{
					var from = currentGuild.GetAllMembersAsync().Result.ToDictionary(s => s.Id);
					var difference = new CollectionDifference<DiscordMember, DiscordMember, ulong>(from.Values, membersFrame.GetObjects(), s => s.Id, s => s.Id);
					foreach (var item in difference.CalculateDifference())
						if (item.Type == CollectionDifference.OperationType.Add)
						{
							var member = from[item.Key];
							membersFrame.AddObject(item.Key, member);
							events.GetRegistry<IMember>().InvokeCreated(GetMember(item.Key), false);
						}
						else
						{
							membersFrame.DeleteObject(item.Key);
							events.GetRegistry<IMember>().InvokeDeleted(item.Key);
						}
				}

				var rolesFrame = serverCache.GetFrame<DiscordRole>();
				lock (rolesFrame)
				{
					var from = currentGuild.Roles;
					var difference = new CollectionDifference<DiscordRole, DiscordRole, ulong>(from.Values.ToArray(), rolesFrame.GetObjects(), s => s.Id, s => s.Id);
					foreach (var item in difference.CalculateDifference())
						if (item.Type == CollectionDifference.OperationType.Add)
						{
							var role = from[item.Key];
							rolesFrame.AddObject(item.Key, role);
							events.GetRegistry<IRole>().InvokeCreated(GetRole(item.Key), false);
						}
						else
						{
							rolesFrame.DeleteObject(item.Key);
							events.GetRegistry<IRole>().InvokeDeleted(item.Key);
						}
				}

				lock (channels.SyncRoot)
				{
					var newData = guild.GetChannelsAsync().Result;

					var archivedThreads = newData.SelectMany(s => s.ListPublicArchivedThreadsAsync().Result.Threads.Concat(s.ListPrivateArchivedThreadsAsync().Result.Threads));
					var activeThreads = guild.ListActiveThreadsAsync().Result.Threads;
					var from = newData.Concat(options.ThreadCache == Options.ThreadCacheBehavior.CacheAll ? activeThreads.Concat(archivedThreads) : activeThreads).ToDictionary(s => s.Id);

					var difference = new CollectionDifference<DiscordChannel, DiscordChannel, ulong>(from.Values.ToArray(), channels.GetRawChannels(categoriesFilter: null), s => s.Id, s => s.Id);
					foreach (var item in difference.CalculateDifference())
						if (item.Type == CollectionDifference.OperationType.Add)
							channels.UpdateChannel(from[item.Key]);
						else channels.DeleteChannel(item.Key);
				}
			});
		});


		private sealed class MessagesEventsManager
		{
			private static readonly EventId SentEventHandlerErrorID = new(11, "SentEventHandlerError");
			private static readonly EventId DeletedEventHandlerErrorID = new(12, "DeletedEventHandlerError");


			private readonly ConcurrentDictionary<ulong, HashSet<MessageDeletedEventHandler>> messageDeletedHandlers = new();
			private readonly ConcurrentDictionary<ulong, HashSet<MessageSentEventHandler>> messageSentHandlers = new();
			private readonly HashSet<MessageDeletedEventHandler> messageDeletedGHandlers = new();
			private readonly HashSet<MessageSentEventHandler> messageSentGHandlers = new();
			private readonly Server owner;
			private readonly ILogger logger;


			public MessagesEventsManager(ILogger logger, Server owner)
			{
				this.owner = owner;
				this.logger = logger;
			}


			public void AddHandler(DiscordChannel channel, MessageDeletedEventHandler? handler)
			{ lock (messageDeletedHandlers) { messageDeletedHandlers.GetOrAdd(channel.Id, _ => new()).Add(handler ?? throw new NullReferenceException()); } }

			public void AddHandler(DiscordChannel channel, MessageSentEventHandler? handler)
			{ lock (messageSentHandlers) { messageSentHandlers.GetOrAdd(channel.Id, _ => new()).Add(handler ?? throw new NullReferenceException()); } }

			public void AddHandler(MessageDeletedEventHandler? handler)
			{ lock (messageDeletedGHandlers) { messageDeletedGHandlers.Add(handler ?? throw new NullReferenceException()); } }

			public void AddHandler(MessageSentEventHandler? handler)
			{ lock (messageSentGHandlers) { messageSentGHandlers.Add(handler ?? throw new NullReferenceException()); } }

			public void RemoveHandler(DiscordChannel channel, MessageDeletedEventHandler? handler)
			{ lock (messageDeletedHandlers) { messageDeletedHandlers.GetOrAdd(channel.Id, _ => new()).Remove(handler ?? throw new NullReferenceException()); } }

			public void RemoveHandler(DiscordChannel channel, MessageSentEventHandler? handler)
			{ lock (messageSentHandlers) { messageSentHandlers.GetOrAdd(channel.Id, _ => new()).Remove(handler ?? throw new NullReferenceException()); } }

			public void RemoveHandler(MessageDeletedEventHandler? handler)
			{ lock (messageDeletedGHandlers) { messageDeletedGHandlers.Remove(handler ?? throw new NullReferenceException()); } }

			public void RemoveHandler(MessageSentEventHandler? handler)
			{ lock (messageSentGHandlers) { messageSentGHandlers.Remove(handler ?? throw new NullReferenceException()); } }


			public void InvokeSentEvent(IMessage message, bool isModified)
			{
				owner.SourceClient.CultureProvider?.SetupCulture(owner);

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
				owner.SourceClient.CultureProvider?.SetupCulture(owner);

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
				lock (messageDeletedHandlers)
				{
					lock (messageSentHandlers)
					{
						messageDeletedHandlers.TryRemove(channel.Id, out _);
						messageSentHandlers.TryRemove(channel.Id, out _);
					}
				}
			}
		}

		/// <summary>
		/// Options for DidiFrame.Clients.DSharp.Server
		/// </summary>
		public class Options
		{
			/// <summary>
			/// Channel threads caching behavior
			/// </summary>
			public ThreadCacheBehavior ThreadCache { get; set; } = ThreadCacheBehavior.CacheActive;


			/// <summary>
			/// Channel threads caching behavior
			/// </summary>
			public enum ThreadCacheBehavior
			{
				/// <summary>
				/// Cache every thread in server
				/// </summary>
				CacheAll,
				/// <summary>
				/// Cache only active thread in server, all archived threads won't exist
				/// </summary>
				CacheActive
			}
		}
	}
}
