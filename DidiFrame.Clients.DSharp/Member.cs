using DidiFrame.Entities;
using DidiFrame.Entities.Message;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IMember
	/// </summary>
	public class Member : User, IMember
	{
		private readonly DiscordMember member;


		/// <inheritdoc/>
		public IServer Server => BaseServer;

		/// <summary>
		/// DSharp server where member present. Casted to DidiFrame.Clients.DSharp.Server Server property
		/// </summary>
		public Server BaseServer { get; }

		/// <inheritdoc/>
		public override string UserName => member.DisplayName;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.Member
		/// </summary>
		/// <param name="member">Base DiscordMember from DSharp</param>
		/// <param name="server">Owner server wrap object</param>
		/// <exception cref="ArgumentException">If base member's server and transmited server wrap are different</exception>
		public Member(DiscordMember member, Server server) : base(member, server.SourceClient)
		{
			if (member.Guild.Id != server.Id)
				throw new ArgumentException("Base channel's server and transmited server wrap are different", nameof(server));

			this.member = member;
			BaseServer = server;
		}


		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => other is Member member && base.Equals(member) && member.Server == Server;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as Member);

		/// <inheritdoc/>
		public override int GetHashCode() => HashCode.Combine(Id, Server);

		/// <inheritdoc/>
		public IReadOnlyCollection<IRole> GetRoles() => member.Roles.Select(s => new Role(s, BaseServer)).ToArray();

		/// <inheritdoc/>
		public Task GrantRoleAsync(IRole role) => BaseServer.SourceClient.DoSafeOperationAsync(() => member.GrantRoleAsync(((Role)role).BaseRole));

		/// <inheritdoc/>
		public Task RevokeRoleAsync(IRole role) => BaseServer.SourceClient.DoSafeOperationAsync(() => member.RevokeRoleAsync(((Role)role).BaseRole));

		/// <inheritdoc/>
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