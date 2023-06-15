using DidiFrame.Entities;
using DidiFrame.Threading;
using DidiFrame.Utils;
using DidiFrame.Utils.RoutedEvents;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DidiFrame.Clients.DSharp
{
	public class Server : IServer
	{
		private readonly RoutedEventTreeNode routedEventTreeNode;
		private readonly DSharpClient owner;
		private readonly DiscordGuild baseGuild;
		private readonly IManagedThreadExecutionQueue workQueue;
		private readonly ObjectsCache<ulong> cache = new();


		public Server(DSharpClient owner, DiscordGuild baseGuild, IManagedThread thread)
		{
			Thread = thread;
			workQueue = Thread.CreateNewExecutionQueue("work");

			routedEventTreeNode = new RoutedEventTreeNode(this);
			routedEventTreeNode.AttachParent(owner.RoutedEventTreeNode);

			this.owner = owner;
			this.baseGuild = baseGuild;
		}


		public IClient Client => owner;

		public DSharpClient BaseClient => owner;

		public bool IsClosed { get; private set; } = false;

		public ulong Id => baseGuild.Id;

		public DateTimeOffset CreationTimeStamp => baseGuild.CreationTimestamp;

		public IManagedThread Thread { get; }

		public IManagedThreadExecutionQueue WorkQueue => workQueue;


		public ICategory GetCategory(ulong id)
		{
			throw new NotImplementedException();
		}

		public TChannel GetChannel<TChannel>(ulong id) where TChannel : notnull, IChannel
		{
			throw new NotImplementedException();
		}

		public IMember GetMember(ulong id)
		{
			return cache.GetFrame<Member>().GetNullableObject(id) ?? new Member(this, id);
		}

		public IRole GetRole(ulong id)
		{
			return cache.GetFrame<Role>().GetNullableObject(id) ?? new Role(this, id);
		}

		public IReadOnlyCollection<ICategory> ListCategories()
		{
			throw new NotImplementedException();
		}

		public IReadOnlyCollection<ChannelProvider> ListChannels()
		{
			throw new NotImplementedException();
		}

		public IReadOnlyCollection<IMember> ListMembers()
		{
			return cache.GetFrame<Member>().GetObjects();
		}

		public IReadOnlyCollection<IRole> ListRoles()
		{
			return cache.GetFrame<Role>().GetObjects();
		}

		public IServerPermissions ManagePermissions()
		{
			throw new NotImplementedException();
		}

		public void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs => routedEventTreeNode.RemoveListener(routedEvent, handler);

		public void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs => routedEventTreeNode.AddListener(routedEvent, handler);

		public override string? ToString()
		{
			return $"[Discord server ({Id})] {{CreationTimeStamp={CreationTimeStamp}}}";
		}

		internal async ValueTask ShutdownAsync()
		{
			var shutdownQueue = Thread.CreateNewExecutionQueue("shutdown");
			workQueue.Dispatch(() => Thread.SetExecutionQueue(shutdownQueue, closePreviousQueue: false));
			IsClosed = true;

			await shutdownQueue.DispatchAsync(() =>
			{
				owner.BaseClient.GuildMemberAdded -= OnMemberCreated;
				owner.BaseClient.GuildMemberUpdated -= OnMemberUpdated;
				owner.BaseClient.GuildMemberRemoved -= OnMemberDeleted;

				owner.BaseClient.GuildRoleCreated -= OnRoleCreated;
				owner.BaseClient.GuildRoleUpdated -= OnRoleUpdated;
				owner.BaseClient.GuildRoleDeleted -= OnRoleDeleted;
			});

			await routedEventTreeNode.Invoke(IServer.ServerRemoved, new IServer.ServerEventArgs(this), CreateEventHandlerExecutor(shutdownQueue));

			shutdownQueue.Dispatch(Thread.Stop);
		}

		internal async ValueTask InitiateStartupProcedure()
		{
			var startupQueue = Thread.CreateNewExecutionQueue("startup");
			routedEventTreeNode.OverrideHandlerExecutor(CreateEventHandlerExecutor(startupQueue));
			Thread.Begin(startupQueue);

			await startupQueue.DispatchAsync(() => new(InitializeCache()));

			await routedEventTreeNode.Invoke(IServer.ServerCreated, new IServer.ServerEventArgs(this));

			routedEventTreeNode.OverrideHandlerExecutor(CreateEventHandlerExecutor(workQueue));
			startupQueue.Dispatch(() => Thread.SetExecutionQueue(workQueue));
		}

		internal void AttachEventTree(RoutedEventTreeNode node)
		{
			node.AttachParent(routedEventTreeNode);
		}

		internal void RemoveCache(ServerObject serverObject)
		{
			if (serverObject is Member m)
			{
				cache.GetFrame<Member>().DeleteObject(m.Id);
			}
		}

		private RoutedEventTreeNode.HandlerExecutor CreateEventHandlerExecutor(IManagedThreadExecutionQueue queue)
		{
			return handler =>
			{
				return queue.DispatchAsync(() =>
				{
					return handler.Invoke();
				});
			};
		}

		private async Task InitializeCache()
		{
			var members = await baseGuild.GetAllMembersAsync();

			var memberFrame = cache.GetFrame<Member>();
			foreach (var member in members)
			{
				var smember = new Member(this, member);
				memberFrame.AddObject(smember.Id, smember);
			}

			await Task.WhenAll(memberFrame.GetObjects().Select(s => s.InitializeAsync()));

			owner.BaseClient.GuildMemberAdded += OnMemberCreated;
			owner.BaseClient.GuildMemberUpdated += OnMemberUpdated;
			owner.BaseClient.GuildMemberRemoved += OnMemberDeleted;


			var roles = baseGuild.Roles;

			var roleFrame = cache.GetFrame<Role>();
			foreach (var role in roles.Values)
			{
				var srole = new Role(this, role);
				roleFrame.AddObject(srole.Id, srole);
			}

			await Task.WhenAll(roleFrame.GetObjects().Select(s => s.InitializeAsync()));

			owner.BaseClient.GuildRoleCreated += OnRoleCreated;
			owner.BaseClient.GuildRoleUpdated += OnRoleUpdated;
			owner.BaseClient.GuildRoleDeleted += OnRoleDeleted;
		}

		private async Task OnMemberCreated(DiscordClient sender, GuildMemberAddEventArgs e)
		{
			if (e.Guild != baseGuild) return;

			await workQueue.DispatchAsync(() =>
			{
				var targetMember = new Member(this, e.Member);

				cache.AddObject(targetMember.Id, targetMember);

				return new ValueTask(targetMember.InitializeAsync());
			});
		}

		private async Task OnMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
		{
			if (e.Guild != baseGuild) return;

			await workQueue.DispatchAsync(() =>
			{
				var targetMember = cache.GetFrame<Member>().GetNullableObject(e.Member.Id);
				if (targetMember is null) return ValueTask.CompletedTask;

				return new ValueTask(targetMember.MutateAsync(e.Member));
			});
		}

		private async Task OnMemberDeleted(DiscordClient sender, GuildMemberRemoveEventArgs e)
		{
			if (e.Guild != baseGuild) return;

			await workQueue.DispatchAsync(() =>
			{
				var targetMember = cache.GetFrame<Member>().GetNullableObject(e.Member.Id);
				if (targetMember is null) return ValueTask.CompletedTask;

				cache.DeleteObject<Member>(targetMember.Id);

				return new ValueTask(targetMember.MakeDeletedAsync());
			});
		}

		private async Task OnRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
		{
			if (e.Guild != baseGuild) return;

			await workQueue.DispatchAsync(() =>
			{
				var targetRole = new Role(this, e.Role);

				cache.AddObject(targetRole.Id, targetRole);

				return new ValueTask(targetRole.InitializeAsync());
			});
		}

		private async Task OnRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs e)
		{
			if (e.Guild != baseGuild) return;

			await workQueue.DispatchAsync(() =>
			{
				var targetRole = cache.GetFrame<Role>().GetNullableObject(e.RoleAfter.Id);
				if (targetRole is null) return ValueTask.CompletedTask;

				return new ValueTask(targetRole.MutateAsync(e.RoleAfter));
			});
		}

		private async Task OnRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
		{
			if (e.Guild != baseGuild) return;

			await workQueue.DispatchAsync(() =>
			{
				var targetRole = cache.GetFrame<Role>().GetNullableObject(e.Role.Id);
				if (targetRole is null) return ValueTask.CompletedTask;

				cache.DeleteObject<Role>(targetRole.Id);

				return new ValueTask(targetRole.MakeDeletedAsync());
			});
		}
	}
}