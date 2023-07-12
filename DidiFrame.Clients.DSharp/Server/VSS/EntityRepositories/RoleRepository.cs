using DSharpPlus;
using DSharpPlus.EventArgs;
using AEHAdd = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.GuildRoleCreateEventArgs>;
using AEHUpdate = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.GuildRoleUpdateEventArgs>;
using AEHRemove = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.GuildRoleDeleteEventArgs>;
using DidiFrame.Utils;
using DidiFrame.Clients.DSharp.Utils;
using System.Net;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories
{
	public class RoleRepository : IEntityRepository<Role>, IEntityRepository<IRole>
	{
		private readonly DSharpServer server;
		private readonly EventBuffer eventBuffer;
		private readonly Dictionary<ulong, Role> roles = new();
		private Role? everyoneRole;


		public RoleRepository(DSharpServer server, EventBuffer eventBuffer)
		{
			this.server = server;
			this.eventBuffer = eventBuffer;
		}


		private DiscordClient DiscordClient => server.BaseClient.DiscordClient;

		public Role EveryoneRole => everyoneRole ?? throw new InvalidOperationException("Initialize repository before use");


		public IReadOnlyCollection<Role> GetAll() => roles.Values;

		public Role GetById(ulong id) => roles[id];

		public async Task InitializeAsync(CompositeAsyncDisposable postInitializationContainer)
		{
			foreach (var role in server.BaseGuild.Roles)
			{
				var srole = new Role(server, role.Key, this);
				roles.Add(srole.Id, srole);

				postInitializationContainer.PushDisposable(await srole.Initialize(role.Value));
			}

			everyoneRole = GetById(server.Id);

			DiscordClient.GuildRoleCreated += new AEHAdd(OnGuildRoleCreated).SyncIn(server.WorkQueue).FilterServer(server.Id);
			DiscordClient.GuildRoleUpdated += new AEHUpdate(OnGuildRoleUpdated).SyncIn(server.WorkQueue).FilterServer(server.Id);
			DiscordClient.GuildRoleDeleted += new AEHRemove(OnGuildRoleDeleted).SyncIn(server.WorkQueue).FilterServer(server.Id);
		}

		public void PerformTerminate()
		{
			DiscordClient.GuildRoleCreated -= new AEHAdd(OnGuildRoleCreated).SyncIn(server.WorkQueue).FilterServer(server.Id);
			DiscordClient.GuildRoleUpdated -= new AEHUpdate(OnGuildRoleUpdated).SyncIn(server.WorkQueue).FilterServer(server.Id);
			DiscordClient.GuildRoleDeleted -= new AEHRemove(OnGuildRoleDeleted).SyncIn(server.WorkQueue).FilterServer(server.Id);
		}

		public Task TerminateAsync()
		{
			return Task.CompletedTask;
		}

		public Task DeleteAsync(Role role)
		{
			roles.Remove(role.Id);
			return role.Finalize().DisposeAsync().AsTask();
		}

		private Task OnGuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e) => CreateOrUpdateAsync(e.Role);

		private Task OnGuildRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs e) => CreateOrUpdateAsync(e.RoleAfter);

		private Task OnGuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
		{
			if (roles.TryGetValue(e.Role.Id, out var role))
			{
				roles.Remove(role.Id);

				var disposable = role.Finalize();

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}

			return Task.CompletedTask;
		}

		private async Task CreateOrUpdateAsync(DiscordRole role)
		{
			if (roles.TryGetValue(role.Id, out var srole))
			{
				var disposable = srole.Mutate(role);

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}
			else
			{
				srole = new Role(server, role.Id, this);
				roles.Add(srole.Id, srole);

				var disposable = await srole.Initialize(role);

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}
		}

		IReadOnlyCollection<IRole> IEntityRepository<IRole>.GetAll() => GetAll();

		IRole IEntityRepository<IRole>.GetById(ulong id) => GetById(id);
	}
}
