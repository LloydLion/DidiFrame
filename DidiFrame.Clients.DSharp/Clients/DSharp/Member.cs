using DidiFrame.Entities;
using DidiFrame.Entities.Message;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	public class Member : User, IMember
	{
		private readonly DiscordMember member;


		public IServer Server => BaseServer;

		public Server BaseServer { get; }

		public override string UserName => member.DisplayName;


		public Member(DiscordMember member, Server server) : base(member, server.SourceClient)
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


		internal Task SendDirectMessageAsyncInternal(MessageSendModel model)
		{
			return BaseServer.SourceClient.DoSafeOperationAsync(async () =>
			{
				var channel = await member.CreateDmChannelAsync();
				await channel.SendMessageAsync(MessageConverter.ConvertUp(model));
			}, new(DSharp.Client.UserName, Id, base.UserName));
		}
	}
}