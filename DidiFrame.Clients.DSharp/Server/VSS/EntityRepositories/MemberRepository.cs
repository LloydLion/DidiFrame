using DSharpPlus;
using DSharpPlus.EventArgs;
using AEHAdd = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.GuildMemberAddEventArgs>;
using AEHUpdate = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.GuildMemberUpdateEventArgs>;
using AEHRemove = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.GuildMemberRemoveEventArgs>;
using DidiFrame.Utils;

namespace DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories
{
	public class MemberRepository : IEntityRepository<Member>, IEntityRepository<IMember>
	{
		private readonly DSharpServer server;
		private readonly Dictionary<ulong, Member> members = new();


		public MemberRepository(DSharpServer server)
		{
			this.server = server;
		}


		private DiscordClient DiscordClient => server.BaseClient.DiscordClient;


		public IReadOnlyCollection<Member> GetAll() => members.Values;

		public Member GetById(ulong id) => members[id];

		public async Task InitializeAsync()
		{
			var compositeDisposable = new CompositeAsyncDisposable();

			foreach (var member in await server.BaseGuild.GetAllMembersAsync())
			{
				var smember = new Member(server, member.Id, this);
				members.Add(smember.Id, smember);

				compositeDisposable.PushDisposable(await smember.Initialize(member));
			}

			await compositeDisposable.DisposeAsync();

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

		private async Task OnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
		{
			var member = new Member(server, e.Member.Id, this);
			members.Add(member.Id, member);

			await (await member.Initialize(e.Member)).DisposeAsync();
		}

		private Task OnGuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
		{
			if (members.TryGetValue(e.Member.Id, out var member))
			{
				return member.Mutate(e.Member).DisposeAsync().AsTask();
			}

			return Task.CompletedTask;
		}

		private Task OnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
		{
			if (members.TryGetValue(e.Member.Id, out var member))
			{
				return DeleteAsync(member);
			}

			return Task.CompletedTask;
		}

		IReadOnlyCollection<IMember> IEntityRepository<IMember>.GetAll() => GetAll();

		IMember IEntityRepository<IMember>.GetById(ulong id) => GetById(id);
	}
}
