using DSharpPlus;
using DSharpPlus.EventArgs;
using DidiFrame.Utils;
using DidiFrame.Clients.DSharp.Utils;
using DSharpPlus.Entities;
using AEHAdd = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.GuildMemberAddEventArgs>;
using AEHUpdate = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.GuildMemberUpdateEventArgs>;
using AEHRemove = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.GuildMemberRemoveEventArgs>;

namespace DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories
{
	public class MemberRepository : IEntityRepository<Member>, IEntityRepository<IMember>
	{
		private readonly DSharpServer server;
		private readonly EventBuffer eventBuffer;
		private readonly Dictionary<ulong, Member> members = new();


		public MemberRepository(DSharpServer server, RoleRepository roleRepository, EventBuffer eventBuffer)
		{
			this.server = server;
			RoleRepository = roleRepository;
			this.eventBuffer = eventBuffer;
		}


		public RoleRepository RoleRepository { get; }

		private DiscordClient DiscordClient => server.BaseClient.DiscordClient;


		public IReadOnlyCollection<Member> GetAll() => members.Values;

		public Member GetById(ulong id) => members[id];

		public async Task InitializeAsync(CompositeAsyncDisposable postInitializationContainer)
		{
			foreach (var member in await server.BaseGuild.GetAllMembersAsync())
			{
				var smember = new Member(server, member.Id, this);
				members.Add(smember.Id, smember);

				postInitializationContainer.PushDisposable(smember.Initialize(member));
			}

			DiscordClient.GuildMemberAdded += new AEHAdd(OnGuildMemberAdded).SyncIn(server.WorkQueue).FilterServer(server.Id);
			DiscordClient.GuildMemberUpdated += new AEHUpdate(OnGuildMemberUpdated).SyncIn(server.WorkQueue).FilterServer(server.Id);
			DiscordClient.GuildMemberRemoved += new AEHRemove(OnGuildMemberRemoved).SyncIn(server.WorkQueue).FilterServer(server.Id);
		}

		public void PerformTerminate()
		{
			DiscordClient.GuildMemberAdded -= new AEHAdd(OnGuildMemberAdded).SyncIn(server.WorkQueue).FilterServer(server.Id);
			DiscordClient.GuildMemberUpdated -= new AEHUpdate(OnGuildMemberUpdated).SyncIn(server.WorkQueue).FilterServer(server.Id);
			DiscordClient.GuildMemberRemoved -= new AEHRemove(OnGuildMemberRemoved).SyncIn(server.WorkQueue).FilterServer(server.Id);
		}

		public Task TerminateAsync()
		{
			return Task.CompletedTask;
		}

		public Task DeleteAsync(Member member)
		{
			members.Remove(member.Id);
			return member.Finalize().DisposeAsync().AsTask();
		}

		private Task OnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e) => CreateOrUpdateAsync(e.Member);

		private Task OnGuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e) => CreateOrUpdateAsync(e.MemberAfter);

		private Task OnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
		{
			if (members.TryGetValue(e.Member.Id, out var member))
			{
				members.Remove(member.Id);

				var disposable = member.Finalize();

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}

			return Task.CompletedTask;
		}

		private Task CreateOrUpdateAsync(DiscordMember member)
		{
			if (members.TryGetValue(member.Id, out var smember))
			{
				var disposable = smember.Mutate(member);

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}
			else
			{
				smember = new Member(server, member.Id, this);
				members.Add(smember.Id, smember);

				var disposable = smember.Initialize(member);

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}

			return Task.CompletedTask;
		}

		IReadOnlyCollection<IMember> IEntityRepository<IMember>.GetAll() => GetAll();

		IMember IEntityRepository<IMember>.GetById(ulong id) => GetById(id);
	}
}
