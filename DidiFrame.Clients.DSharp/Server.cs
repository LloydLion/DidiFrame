using DidiFrame.Culture;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DidiFrame.Utils;
using DidiFrame.Utils.Collections;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections;

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

		//private readonly List<Member> members = new();
		//private readonly List<ChannelCategory> categories = new();
		//private readonly List<Channel> channels = new();
		//private readonly List<Role> roles = new();
		//private readonly Dictionary<TextChannel, Dictionary<ulong, TextThread>> threads = new();
		//private readonly ThreadLocker<IList> cacheUpdateLocker = new();
		private readonly ThreadsChannelsCollection threadsAndChannels;
		private readonly ChannelCategory globalCategory;
		private readonly ObjectsCache<ulong> serverCache = new();
		private readonly ChannelMessagesCache messages = new();
		private readonly TextChannelThreadsCache threads;


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


		public IReadOnlyCollection<ITextThread> GetThreadsFor(TextChannel channel) => threads.GetThreadsFor(channel);

		/// <inheritdoc/>
		public IMember GetMember(ulong id) => serverCache.GetFrame<Member>().GetObject(id);

		/// <inheritdoc/>
		public IReadOnlyCollection<IMember> GetMembers() => serverCache.GetFrame<Member>().GetObjects();

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannelCategory> GetCategories() => new CategoriesCollection(serverCache.GetFrame<ChannelCategory>(), globalCategory);

		/// <inheritdoc/>
		public IChannelCategory GetCategory(ulong? id) => id is null ? globalCategory : serverCache.GetFrame<ChannelCategory>().GetObject(id.Value);

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannel> GetChannels() => threadsAndChannels;

		/// <inheritdoc/>
		public IChannel GetChannel(ulong id) => threadsAndChannels.GetChannel(id);

		/// <inheritdoc/>
		public IReadOnlyCollection<IRole> GetRoles() => serverCache.GetFrame<Role>().GetObjects();

		/// <inheritdoc/>
		public IRole GetRole(ulong id) => serverCache.GetFrame<Role>().GetObject(id);

		/// <inheritdoc/>
		public bool Equals(IServer? other) => other is Server server && server.Id == Id;

		/// <inheritdoc/>
		public bool HasChannel(ulong id) => threadsAndChannels.HasChannel(id);

		/// <inheritdoc/>
		public bool HasChannel(IChannel channel) => threadsAndChannels.HasChannel(channel.Id);

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as Server);

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();

		/// <inheritdoc/>
		public void Dispose()
		{
			//Detatch events
			client.BaseClient.GuildMemberAdded -= OnGuildMemberAdded;
			client.BaseClient.GuildRoleCreated -= OnGuildRoleCreated;
			client.BaseClient.ChannelCreated -= OnChannelCreated;
			client.BaseClient.MessageCreated -= OnMessageCreated;

			client.BaseClient.GuildMemberRemoved -= OnGuildMemberRemoved;
			client.BaseClient.GuildRoleDeleted -= OnGuildRoleDeleted;
			client.BaseClient.ChannelDeleted -= OnChannelDeleted;
			client.BaseClient.MessageDeleted -= OnMessageDeleted;

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


			threads = new(
			(thread) =>
			{
				messages.InitChannelAsync(thread).Wait();
			},

			(thread) =>
			{
				messages.DeleteChannelCache(thread);
			});

			serverCache.SetRemoveActionHandler<Channel>((id, channel) =>
			{
				if (channel is TextChannelBase btch) messages.DeleteChannelCache(btch);
				if (channel is TextChannel tch) threads.RemoveAllThreadsThatAssociatedWith(tch);
			});

			serverCache.SetAddActionHandler<Channel>((id, channel) =>
			{
				if (channel is TextChannelBase btch) messages.InitChannelAsync(btch).Wait();
			});

			threadsAndChannels = new(serverCache.GetFrame<Channel>(), threads);


			client.BaseClient.GuildMemberAdded += OnGuildMemberAdded;
			client.BaseClient.GuildRoleCreated += OnGuildRoleCreated;
			client.BaseClient.ChannelCreated += OnChannelCreated;
			client.BaseClient.MessageCreated += OnMessageCreated;
			client.BaseClient.ThreadCreated += OnThreadCreated;

			client.BaseClient.GuildMemberRemoved += OnGuildMemberRemoved;
			client.BaseClient.GuildRoleDeleted += OnGuildRoleDeleted;
			client.BaseClient.ChannelDeleted += OnChannelDeleted;
			client.BaseClient.MessageDeleted += OnMessageDeleted;
			client.BaseClient.ThreadDeleted += OnThreadDeleted;
			
			client.BaseClient.MessageUpdated += OnMessageUpdated;


			globalCacheUpdateTask = CreateServerCacheUpdateTask(cts.Token);
		}


		private Task OnMessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;
			if (e.Message.Author.Id == Client.SelfAccount.Id) return Task.CompletedTask;

			var channel = (TextChannelBase)GetChannel(e.Channel.Id);
			if (messages.HasMessage(e.Message.Id, channel))
			{
				var ready = messages.GetReadyMessage(e.Message.Id, channel);
				if (ready is not null) ready.ModifyInternal(e.Message);
			}

			return Task.CompletedTask;
		}

		private Task OnThreadDeleted(DiscordClient sender, ThreadDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			lock (threads)
			{
				threads.RemoveThread(e.Thread.Id);
			}

			return Task.CompletedTask;
		}

		private Task OnThreadCreated(DiscordClient sender, ThreadCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			lock (threads)
			{
				var thread = new TextThread(e.Thread, this, () => (ChannelCategory)GetCategory(e.Thread.Parent.ParentId));

				threads.AddThread(thread);
			}

			return Task.CompletedTask;
		}

		private Task OnChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			if (e.Channel.IsCategory)
				serverCache.DeleteObject<ChannelCategory>(e.Channel.Id);
			else serverCache.DeleteObject<Channel>(e.Channel.Id);
			return Task.CompletedTask;
		}

		private Task OnGuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.DeleteObject<Role>(e.Role.Id);

			return Task.CompletedTask;
		}

		private Task OnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.DeleteObject<Member>(e.Member.Id);

			return Task.CompletedTask;
		}

		private Task OnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			var channel = (TextChannelBase)GetChannel(e.Message.ChannelId);
			var deletedMsg = messages.DeleteMessage(e.Message.Id, channel);
			client.CultureProvider.SetupCulture(this);
			SourceClient.OnMessageDeleted(deletedMsg);
			return Task.CompletedTask;
		}

		private async Task OnChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
		{
			if (e.Guild != guild) return;

			if (e.Channel.IsCategory)
				serverCache.AddObject(e.Channel.Id, new ChannelCategory(e.Channel, this));
			else
				serverCache.AddObject(e.Channel.Id, Channel.Construct(e.Channel, this));
		}

		private Task OnGuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.AddObject(e.Role.Id, new Role(e.Role, this));

			return Task.CompletedTask;
		}

		private Task OnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			serverCache.AddObject(e.Member.Id, new Member(e.Member, this));

			return Task.CompletedTask;
		}

		private Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			var channel = (TextChannelBase)GetChannel(e.Channel.Id);
			var model = MessageConverter.ConvertDown(e.Message);
			var message = new Message(e.Message, channel, model);
			messages.AddMessage(message);
			client.CultureProvider.SetupCulture(this);
			SourceClient.OnMessageCreated(message, false);
			return Task.CompletedTask;
		}

		private async Task CreateServerCacheUpdateTask(CancellationToken token)
		{
			update().Wait(token);

			while (token.IsCancellationRequested)
			{
				await update();
				await Task.Delay(new TimeSpan(0, 5, 0), token);
			}


			Task update() => client.DoSafeOperationAsync(async () =>
				{
					var memebersFrame = serverCache.GetFrame<Member>();
					lock (memebersFrame)
					{
						var from = guild.GetAllMembersAsync().Result.ToDictionary(s => s.Id);
						var difference = new CollectionDifference<DiscordMember, Member, ulong>(from.Values, memebersFrame.GetObjects(), s => s.Id, s => s.Id);
						foreach (var item in difference.CalculateDifference())
							if (item.Type == CollectionDifference<DiscordMember, Member, ulong>.OperationType.Add)
							{
								var member = from[item.Key];
								memebersFrame.AddObject(item.Key, new(member, this));
							}
							else memebersFrame.DeleteObject(item.Key);
					}

					var rolesFrame = serverCache.GetFrame<Role>();
					lock (rolesFrame)
					{
						var from = guild.Roles;
						var difference = new CollectionDifference<DiscordRole, Role, ulong>(from.Values.ToArray(), rolesFrame.GetObjects(), s => s.Id, s => s.Id);
						foreach (var item in difference.CalculateDifference())
							if (item.Type == CollectionDifference<DiscordRole, Role, ulong>.OperationType.Add)
							{
								var role = from[item.Key];
								rolesFrame.AddObject(item.Key, new(role, this));
							}
							else rolesFrame.DeleteObject(item.Key);
					}

					var channelsFrame = serverCache.GetFrame<Channel>();
					var categoriesFrame = serverCache.GetFrame<ChannelCategory>();
					lock (channelsFrame)
					{
						lock (categoriesFrame)
						{
							lock (threads)
							{
								var newData = guild.GetChannelsAsync().Result;

								{
									var from = newData.Where(s => s.IsCategory).ToDictionary(s => s.Id);
									var difference = new CollectionDifference<DiscordChannel, ChannelCategory, ulong>(from.Values.ToArray(), categoriesFrame.GetObjects(), s => s.Id, s => s.Id ?? throw new ImpossibleVariantException());
									foreach (var item in difference.CalculateDifference())
										if (item.Type == CollectionDifference<DiscordChannel, ChannelCategory, ulong>.OperationType.Add)
										{
											var category = from[item.Key];
											categoriesFrame.AddObject(item.Key, new(category, this));
										}
										else categoriesFrame.DeleteObject(item.Key);
								}

								{
									var from = newData.Where(s => !s.IsCategory).ToDictionary(s => s.Id);
									var difference = new CollectionDifference<DiscordChannel, Channel, ulong>(from.Values.ToArray(), channelsFrame.GetObjects(), s => s.Id, s => s.Id);
									foreach (var item in difference.CalculateDifference())
										if (item.Type == CollectionDifference<DiscordChannel, Channel, ulong>.OperationType.Add)
										{
											var channel = from[item.Key];
											var readyChannel = Channel.Construct(channel, this);
											channelsFrame.AddObject(item.Key, readyChannel);

											//Load threads
											if (readyChannel is TextChannel tch)
											{
												var newThreads = channel.Threads.ToDictionary(s => s.Id);
												var threadDiff = new CollectionDifference<DiscordThreadChannel, TextThread, ulong>(newThreads.Values, threads.GetThreads(), s => s.Id, s => s.Id);
												foreach (var diff in threadDiff.CalculateDifference())
													if (diff.Type == CollectionDifference<DiscordThreadChannel, TextThread, ulong>.OperationType.Add)
														threads.AddThread(new TextThread(newThreads[diff.Key], this, () => (ChannelCategory)readyChannel.Category));
													else threads.RemoveThread(diff.Key);
											}
										}
										else channelsFrame.DeleteObject(item.Key);
								}
							}
						}
					}
				});
		}


		private class ThreadsChannelsCollection : IReadOnlyCollection<IChannel>
		{
			private readonly ObjectsCache<ulong>.Frame<Channel> channels;
			private readonly TextChannelThreadsCache threads;


			public int Count => channels.GetObjects().Count + threads.GetThreads().Count;


			public ThreadsChannelsCollection(ObjectsCache<ulong>.Frame<Channel> channels, TextChannelThreadsCache threads)
			{
				this.channels = channels;
				this.threads = threads;
			}


			public IEnumerator<IChannel> GetEnumerator()
			{
				lock (channels)
				{
					lock (threads)
					{
						foreach (var item in channels.GetObjects()) yield return item;
						foreach (var item in threads.GetThreads()) yield return item;
					}
				}
			}

			public IChannel GetChannel(ulong id)
			{
				lock (channels)
				{
					lock (threads)
					{
						if (channels.HasObject(id)) return channels.GetObject(id);
						else return threads.GetThread(id);
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
			private readonly Dictionary<ulong, TextThread> threads = new();
			private readonly Action<TextThread> addAction;
			private readonly Action<TextThread> removeAction;


			public TextChannelThreadsCache(Action<TextThread> addAction, Action<TextThread> removeAction)
			{
				this.addAction = addAction;
				this.removeAction = removeAction;
			}


			public void AddThread(TextThread thread)
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

			public TextThread GetThread(ulong threadId)
			{
				lock (this)
				{
					return threads[threadId];
				}
			}

			public IReadOnlyCollection<TextThread> GetThreads()
			{
				lock (this)
				{
					return threads.Values;
				}
			}

			public void RemoveAllThreadsThatAssociatedWith(TextChannel channel)
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

			public IReadOnlyCollection<ITextThread> GetThreadsFor(TextChannel channel)
			{
				lock (this)
				{
					var toRet = new List<TextThread>();

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


			public CategoriesCollection(ObjectsCache<ulong>.Frame<ChannelCategory> frame, ChannelCategory globalCategory)
			{
				this.globalCategory = globalCategory;
				categories = frame.GetObjects();
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
