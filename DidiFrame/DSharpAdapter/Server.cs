using DidiFrame.Culture;
using DidiFrame.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections;

namespace DidiFrame.DSharpAdapter
{
	internal class Server : IServer, IDisposable
	{
		private readonly DiscordGuild guild;
		private readonly Client client;
		private readonly Task globalCacheUpdateTask;
		private readonly CancellationTokenSource cts = new();

		private readonly List<Member> members = new();
		private readonly List<ChannelCategory> categories = new();
		private readonly List<Channel> channels = new();
		private readonly List<Role> roles = new();

		private readonly ThreadLocker<IList> cacheUpdateLocker = new();


		public string Name => guild.Name;

		public IClient Client => client;

		public Client SourceClient => client;

		public ulong Id => guild.Id;

		public DiscordGuild Guild => guild;


		public IMember GetMember(ulong id) => GetMembers().Single(s => s.Id == id);

		public IReadOnlyCollection<IMember> GetMembers() => members;

		public IReadOnlyCollection<IChannelCategory> GetCategories() => categories;

		public IChannelCategory GetCategory(ulong? id) => GetCategories().Single(s => s.Id == id);

		public IReadOnlyCollection<IChannel> GetChannels() => channels;

		public IChannel GetChannel(ulong id) => GetChannels().Single(s => s.Id == id);

		public IReadOnlyCollection<IRole> GetRoles() => roles;

		public IRole GetRole(ulong id) => GetRoles().Single(s => s.Id == id);

		public bool Equals(IServer? other) => other is Server server && server.Id == Id;

		public bool HasChannel(ulong id) => GetChannels().Any(s => s.Id == id);

		public bool HasChannel(IChannel channel) => GetChannels().Contains(channel);

		public override bool Equals(object? obj) => Equals(obj as Server);

		public override int GetHashCode() => Id.GetHashCode();

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
		}


		public Server(DiscordGuild guild, Client client)
		{
			this.guild = guild;
			this.client = client;

			categories.Add(new ChannelCategory(this)); //Global category

			client.BaseClient.GuildMemberAdded += OnGuildMemberAdded;
			client.BaseClient.GuildRoleCreated += OnGuildRoleCreated;
			client.BaseClient.ChannelCreated += OnChannelCreated;
			client.BaseClient.MessageCreated += OnMessageCreated;

			client.BaseClient.GuildMemberRemoved += OnGuildMemberRemoved;
			client.BaseClient.GuildRoleDeleted += OnGuildRoleDeleted;
			client.BaseClient.ChannelDeleted += OnChannelDeleted;
			client.BaseClient.MessageDeleted += OnMessageDeleted;

			globalCacheUpdateTask = CreateServerCacheUpdateTask(cts.Token);
		}

		private Task OnChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			if (e.Channel.IsCategory)
				using (cacheUpdateLocker.Lock(roles))
					categories.RemoveAll(s => s.Id == e.Channel.Id);
			else
				using (cacheUpdateLocker.Lock(roles))
					channels.RemoveAll(s => s.Id == e.Channel.Id);
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

			using (cacheUpdateLocker.Lock(roles))
				members.RemoveAll(s => s.Id == e.Member.Id);
			return Task.CompletedTask;
		}

		private Task OnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			var channel = (TextChannel)GetChannel(e.Message.ChannelId);
			client.CultureProvider.SetupCulture(this);
			channel.OnMessageDelete(e.Message);
			return Task.CompletedTask;
		}

		private Task OnChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			if (e.Channel.IsCategory)
				using (cacheUpdateLocker.Lock(roles))
					categories.Add(new ChannelCategory(e.Channel, this));
			else
				using (cacheUpdateLocker.Lock(roles))
					channels.Add(Channel.Construct(e.Channel, this));
			return Task.CompletedTask;
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

			using (cacheUpdateLocker.Lock(roles))
				members.Add(new Member(e.Member, this));
			return Task.CompletedTask;
		}

		private Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
		{
			if (e.Guild != guild) return Task.CompletedTask;

			var channel = (TextChannel)GetChannel(e.Channel.Id);
			client.CultureProvider.SetupCulture(this);
			channel.OnMessageCreate(e.Message);
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
						members.Clear();
						members.AddRange((await guild.GetAllMembersAsync()).Select(s => new Member(s, this)));
					}

					using (cacheUpdateLocker.Lock(roles))
					{
						roles.Clear();
						roles.AddRange(guild.Roles.Select(s => new Role(s.Value, this)));
					}

					using (cacheUpdateLocker.Lock(channels))
					{
						using (cacheUpdateLocker.Lock(categories))
						{
							var chs = await guild.GetChannelsAsync();
							channels.Clear();
							categories.Clear();
							foreach (var ch in chs)
							{
								if (ch.IsCategory) categories.Add(new ChannelCategory(ch, this));
								else channels.Add(Channel.Construct(ch, this));
							}
						}
					}
				});
		}
	}
}
