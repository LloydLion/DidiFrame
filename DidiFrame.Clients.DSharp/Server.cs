using DidiFrame.Culture;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DidiFrame.Utils;
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

		private readonly List<Member> members = new();
		private readonly List<ChannelCategory> categories = new();
		private readonly List<Channel> channels = new();
		private readonly List<Role> roles = new();
		private readonly Dictionary<TextChannel, Dictionary<ulong, TextThread>> threads = new();
		private readonly ThreadsChannelsCollection threadsAndChannels;
		private readonly ChannelMessagesCache messages = new();
		private readonly ThreadLocker<IList> cacheUpdateLocker = new();


		/// <inheritdoc/>
		public event MessageDeletedEventHandler MessageDeleted
		{
			add => SourceClient.MessageDeleted += new MessageDeletedSubscriber(Id, value).Handle;
			remove => SourceClient.MessageDeleted -= new MessageDeletedSubscriber(Id, value).Handle;
		}

		/// <inheritdoc/>
		public event MessageSentEventHandler MessageSent
		{
			add => SourceClient.MessageDeleted += new MessageSentSubscriber(Id, value).Handle;
			remove => SourceClient.MessageDeleted -= new MessageSentSubscriber(Id, value).Handle;
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


		public IReadOnlyCollection<ITextThread> GetThreadsFor(TextChannel channel) => threads[channel].Values;

		public ITextChannel GetBaseChannel(TextThread thread) => threads.Single(s => s.Value.ContainsKey(thread.Id)).Key;

		/// <inheritdoc/>
		public IMember GetMember(ulong id) => GetMembers().Single(s => s.Id == id);

		/// <inheritdoc/>
		public IReadOnlyCollection<IMember> GetMembers() => members;

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannelCategory> GetCategories() => categories;

		/// <inheritdoc/>
		public IChannelCategory GetCategory(ulong? id) => GetCategories().Single(s => s.Id == id);

		/// <inheritdoc/>
		public IReadOnlyCollection<IChannel> GetChannels() => threadsAndChannels;

		/// <inheritdoc/>
		public IChannel GetChannel(ulong id) => GetChannels().Single(s => s.Id == id);

		/// <inheritdoc/>
		public IReadOnlyCollection<IRole> GetRoles() => roles;

		/// <inheritdoc/>
		public IRole GetRole(ulong id) => GetRoles().Single(s => s.Id == id);

		/// <inheritdoc/>
		public bool Equals(IServer? other) => other is Server server && server.Id == Id;

		/// <inheritdoc/>
		public bool HasChannel(ulong id) => GetChannels().Any(s => s.Id == id);

		/// <inheritdoc/>
		public bool HasChannel(IChannel channel) => GetChannels().Contains(channel);

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

			threadsAndChannels = new(channels, threads);

			categories.Add(new ChannelCategory(this)); //Global category

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

			globalCacheUpdateTask = CreateServerCacheUpdateTask(cts.Token);
		}


		private Task OnThreadDeleted(DiscordClient sender, ThreadDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			using (cacheUpdateLocker.Lock(channels))
			{
				var tch = (TextChannel)GetChannel(e.Parent.Id);
				var cache = threads[tch];
				cache.Remove(e.Thread.Id);
			}

			return Task.CompletedTask;
		}

		private Task OnThreadCreated(DiscordClient sender, ThreadCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			using (cacheUpdateLocker.Lock(channels))
			{
				var tch = (TextChannel)GetChannel(e.Parent.Id);
				var cache = threads[tch];
				cache.Add(e.Thread.Id, new(e.Thread, this, () => (ChannelCategory)GetCategory(e.Parent.ParentId)));
			}

			return Task.CompletedTask;
		}

		private Task OnChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			if (e.Channel.IsCategory)
				using (cacheUpdateLocker.Lock(categories))
					categories.RemoveAll(s => s.Id == e.Channel.Id);
			else
				using (cacheUpdateLocker.Lock(channels))
				{
					var channel = (TextChannel)GetChannel(e.Channel.Id);
					channels.Remove(channel);
					if (channel is TextChannelBase btch)
					{
						messages.DeleteChannelCache(btch);
						if (channel is TextChannel tch) threads.Remove(tch);
					}
				}
			return Task.CompletedTask;
		}

		private Task OnGuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			using (cacheUpdateLocker.Lock(roles))
				roles.RemoveAll(s => s.Id == e.Role.Id);
			return Task.CompletedTask;
		}

		private Task OnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			using (cacheUpdateLocker.Lock(members))
				members.RemoveAll(s => s.Id == e.Member.Id);
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
				using (cacheUpdateLocker.Lock(categories))
					categories.Add(new ChannelCategory(e.Channel, this));
			else
				using (cacheUpdateLocker.Lock(channels))
				{
					var channel = Channel.Construct(e.Channel, this);
					channels.Add(channel);
					if (channel is TextChannelBase btch)
					{
						await messages.InitChannelAsync(btch);
						if (channel is TextChannel tch) threads.Add(tch, new());
					}
				}
		}

		private Task OnGuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			using (cacheUpdateLocker.Lock(roles))
				roles.Add(new Role(e.Role, this));
			return Task.CompletedTask;
		}

		private Task OnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			using (cacheUpdateLocker.Lock(members))
				members.Add(new Member(e.Member, this));
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
			SourceClient.OnMessageCreated(message);
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
					using (cacheUpdateLocker.Lock(members))
					{
						var temp = members.ToList();
						members.Clear();
						var newMembers = await guild.GetAllMembersAsync();

						foreach (var item in newMembers)
						{
							var maybe = temp.SingleOrDefault(s => s.Id == item.Id);
							if (maybe is not null)
							{
								members.Add(maybe);
								temp.Remove(maybe);
							}
							else members.Add(new Member(item, this));
						}
					}

					using (cacheUpdateLocker.Lock(roles))
					{
						var temp = roles.ToList();
						roles.Clear();
						var newRoles = guild.Roles;

						foreach (var item in newRoles)
						{
							var maybe = temp.SingleOrDefault(s => s.Id == item.Key);
							if (maybe is not null)
							{
								roles.Add(maybe);
								temp.Remove(maybe);
							}
							else roles.Add(new Role(item.Value, this));
						}
					}

					using (cacheUpdateLocker.Lock(channels))
					{
						using (cacheUpdateLocker.Lock(categories))
						{
							var newChannels = await guild.GetChannelsAsync();

							var catTmp = categories.Where(s => s.Id.HasValue).ToDictionary(s => s.Id ?? throw new ImpossibleVariantException());
							var chTmp = channels.ToDictionary(s => s.Id);

							categories.RemoveAll(s => s.Id.HasValue); //Remove all except global category
							channels.Clear();

							foreach (var baseChannel in newChannels)
							{
								if (baseChannel.IsCategory)
								{
									//If category was in list readd else create new
									if (catTmp.TryGetValue(baseChannel.Id, out var sod))
									{
										categories.Add(sod);
										catTmp.Remove(baseChannel.Id);
									}
									else categories.Add(new ChannelCategory(baseChannel, this));
								}
								else
								{
									//If channel was in list readd else create new
									if (chTmp.TryGetValue(baseChannel.Id, out var sod))
										chTmp.Remove(baseChannel.Id);
									else sod = Channel.Construct(baseChannel, this);

									channels.Add(sod);

									if (sod is TextChannelBase btch) //If channel is support text mode add it to messages cache
									{
										await messages.InitChannelAsync(btch);
										if (sod is TextChannel tch) //If channel is text add it to threads cache
										{
											Dictionary<ulong, TextThread> cache;
											//Get or create cache dictionary
											if (threads.ContainsKey(tch)) cache = threads[tch];
											else threads.Add(tch, cache = new());


											var cacheClone = cache.ToDictionary(s => s.Key, s => s.Value);
											cache.Clear();

											foreach (var thread in baseChannel.Threads)
											{
												//If thread was in list readd else create new
												if (cacheClone.TryGetValue(thread.Id, out var val))
												{
													cache.Add(thread.Id, val);
													cacheClone.Remove(thread.Id);
												}
												else cache.Add(thread.Id, new TextThread(thread, this, () => (ChannelCategory)GetCategory(baseChannel.ParentId)));
											}
										}
									}
								}
							}

							//Delete caches for all deleted text-like channels
							foreach (var deletedChannel in chTmp)
								if (deletedChannel.Value is TextChannelBase btch)
								{
									messages.DeleteChannelCache(btch);
									if (deletedChannel.Value is TextChannel tch)
										threads.Remove(tch);
								}
						}
					}
				});
		}


		private class ThreadsChannelsCollection : IReadOnlyCollection<IChannel>
		{
			private readonly List<Channel> channels;
			private readonly Dictionary<TextChannel, Dictionary<ulong, TextThread>> threads;


			public int Count => channels.Count + threads.Select(s => s.Value.Count).Sum();


			public ThreadsChannelsCollection(List<Channel> channels, Dictionary<TextChannel, Dictionary<ulong, TextThread>> threads)
			{
				this.channels = channels;
				this.threads = threads;
			}


			public IEnumerator<IChannel> GetEnumerator()
			{
				foreach (var item in channels) yield return item;
				foreach (var pair in threads.Values)
					foreach (var item in pair.Values) yield return item;
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


			public void Handle(IClient client, IMessage message)
			{
				if (message.TextChannel.Server.Id == serverId)
					handler.Invoke(client, message);
			}
		}
	}
}
