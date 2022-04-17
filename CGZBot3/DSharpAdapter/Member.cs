using CGZBot3.Entities.Message;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class Member : User, IMember
	{
		private readonly DiscordMember member;
		private readonly MessageConverter converter = new();


		public IServer Server => BaseServer;

		public Server BaseServer { get; }

		public override string UserName => member.DisplayName;


		public Member(DiscordMember member, Server server) : base(member, (Client)server.Client)
		{
			this.member = member;
			BaseServer = server;
		}


		public bool Equals(IServerEntity? other) => other is Member member && member.Id == Id;

		public IReadOnlyCollection<IRole> GetRoles() => member.Roles.Select(s => new Role(s, BaseServer)).ToArray();

		public Task GrantRoleAsync(IRole role) => member.GrantRoleAsync(((Role)role).BaseRole);

		public Task RevokeRoleAsync(IRole role) => member.RevokeRoleAsync(((Role)role).BaseRole);

		public bool HasPermissionIn(Permissions permissions, IChannel channel) =>
			member.PermissionsIn(((Channel)channel).BaseChannel).GetAbstract().HasFlag(permissions);

		public async Task SendDirectMessageAsync(MessageSendModel model)
		{
			var channel = await member.CreateDmChannelAsync();
			await channel.SendMessageAsync(converter.ConvertUp(model));
		}
	}
}