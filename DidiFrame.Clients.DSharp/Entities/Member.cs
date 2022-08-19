using DidiFrame.Entities;
using DidiFrame.Entities.Message;
using DidiFrame.Exceptions;
using DidiFrame.Client;
using DSharpPlus.Entities;
using System.Runtime.CompilerServices;

namespace DidiFrame.Client.DSharp.Entities
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IMember
	/// </summary>
	public sealed class Member : User, IMember
	{
		private readonly ObjectSourceDelegate<DiscordMember> member;
		private readonly Server baseServer;


		/// <inheritdoc/>
		public IServer Server => baseServer;

		/// <summary>
		/// DSharp server where member present. Casted to DidiFrame.Clients.DSharp.Server Server property
		/// </summary>
		public Server BaseServer => baseServer;

		/// <inheritdoc/>
		public override string UserName => AccessBase().Username;

		/// <summary>
		/// Base DiscordMember from DSharp
		/// </summary>
		public DiscordMember BaseMember => AccessBase();

		/// <inheritdoc/>
		public bool IsExist => member() is not null;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.Member
		/// </summary>
		/// <param name="id">Id of user and member</param>
		/// <param name="member">Base DiscordMember from DSharp source</param>
		/// <param name="server">Owner server wrap object</param>
		public Member(ulong id, ObjectSourceDelegate<DiscordMember> member, Server server) : base(id, member, server.SourceClient)
		{
			baseServer = server;
			this.member = member;
		}


		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => Equals(other as Member);

		/// <inheritdoc/>
		public bool Equals(IMember? other) => other is Member otherMember && otherMember.IsExist && IsExist && base.Equals(otherMember) && otherMember.Server == Server;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as Member);


		/// <inheritdoc/>
		public override int GetHashCode() => HashCode.Combine(Id, Server);

		/// <inheritdoc/>
		public IReadOnlyCollection<IRole> GetRoles()
		{
			return AccessBase().Roles.Select(s => baseServer.GetRole(s.Id)).ToArray();
		}

		/// <inheritdoc/>
		public Task GrantRoleAsync(IRole role)
		{
			return BaseServer.SourceClient.DoSafeOperationAsync(() => AccessBase().GrantRoleAsync(((Role)role).BaseRole));
		}

		/// <inheritdoc/>
		public Task RevokeRoleAsync(IRole role)
		{
			return BaseServer.SourceClient.DoSafeOperationAsync(() => AccessBase().RevokeRoleAsync(((Role)role).BaseRole));
		}

		/// <inheritdoc/>
		public bool HasPermissionIn(Permissions permissions, IChannel channel) =>
			AccessBase().PermissionsIn(((Channel)channel).BaseChannel).GetAbstract().HasFlag(permissions);

		internal Task SendDirectMessageAsyncInternal(MessageSendModel model)
		{
			return BaseServer.SourceClient.DoSafeOperationAsync(async () =>
			{
				var channel = await AccessBase().CreateDmChannelAsync();
				await channel.SendMessageAsync(MessageConverter.ConvertUp(model));
			}, new(DSharp.DSharpClient.UserName, Id, base.UserName));
		}

		private DiscordMember AccessBase([CallerMemberName] string nameOfCaller = "")
		{
			var obj = member();
			if (obj is null)
				throw new ObjectDoesNotExistException(nameOfCaller);
			else return obj;
		}
	}
}