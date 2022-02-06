using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class Member : User, IMember
	{
		private readonly DiscordMember member;


		public IServer Server { get; }

		public override string UserName => member.DisplayName;


		public Member(DiscordMember member, Server server) : base(member, (Client)server.Client)
		{
			this.member = member;
			Server = server;
		}


		public bool Equals(IServerEntity? other) => other is Member member && member.Id == Id;

		public Task<IReadOnlyCollection<IRole>> GetRolesAsync() => Task.FromResult((IReadOnlyCollection<IRole>)member.Roles.ToArray());

		public Task GrantRoleAsync(IRole role) => member.GrantRoleAsync(((Role)role).BaseRole);

		public Task RevokeRoleAsync(IRole role) => member.RevokeRoleAsync(((Role)role).BaseRole);

		public bool HasPermissionIn(Permissions permissions, IChannel channel) =>
			member.PermissionsIn(((Channel)channel).BaseChannel).GetAbstract().HasFlag(permissions);
	}
}