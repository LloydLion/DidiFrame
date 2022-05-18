using DSharpPlus.Entities;

namespace DidiFrame.DSharpAdapter
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


		public bool Equals(IServerEntity? other) => other is Member member && base.Equals(member) && member.Server == Server;

		public override bool Equals(object? obj) => Equals(obj as Member);

		public override int GetHashCode() => HashCode.Combine(Id, Server);

		public IReadOnlyCollection<IRole> GetRoles() => member.Roles.Select(s => new Role(s, BaseServer)).ToArray();

		public Task GrantRoleAsync(IRole role) => BaseServer.SourceClient.DoSafeOperationAsync(() => member.GrantRoleAsync(((Role)role).BaseRole));

		public Task RevokeRoleAsync(IRole role) => BaseServer.SourceClient.DoSafeOperationAsync(() => member.RevokeRoleAsync(((Role)role).BaseRole));

		public bool HasPermissionIn(Permissions permissions, IChannel channel) =>
			member.PermissionsIn(((Channel)channel).BaseChannel).GetAbstract().HasFlag(permissions);


		public Task SendDirectMessageAsyncInternal(MessageSendModel model)
		{
			return BaseServer.SourceClient.DoSafeOperationAsync(async () =>
			{
				var channel = await member.CreateDmChannelAsync();
				await channel.SendMessageAsync(converter.ConvertUp(model));
			});
		}
	}
}