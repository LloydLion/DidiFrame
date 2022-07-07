using DidiFrame.Entities;
using DidiFrame.Entities.Message;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IMember
	/// </summary>
	public class Member : User, IMember
	{
		private DiscordMember member;
		private readonly Server baseServer;


		/// <inheritdoc/>
		public IServer Server => baseServer;

		/// <summary>
		/// DSharp server where member present. Casted to DidiFrame.Clients.DSharp.Server Server property
		/// </summary>
		public Server BaseServer => baseServer;

		/// <inheritdoc/>
		public override string UserName
		{
			get
			{
				lock (this)
				{
					if (IsExist == false)
						throw new ObjectDoesNotExistException(nameof(UserName));
					return member.DisplayName;
				}
			}
		}

		/// <summary>
		/// Base DiscordMember from DSharp
		/// </summary>
		public DiscordMember BaseMember
		{
			get
			{
				lock (this)
				{
					if (IsExist == false)
						throw new ObjectDoesNotExistException(nameof(BaseMember));
					return member;
				}
			}
		}

		/// <inheritdoc/>
		public bool IsExist => baseServer.HasMember(this);


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
			baseServer = server;
		}


		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => other is Member member && member.IsExist && IsExist && base.Equals(member) && member.Server == Server;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as Member);

		/// <inheritdoc/>
		public override int GetHashCode() => HashCode.Combine(Id, Server);

		/// <inheritdoc/>
		public IReadOnlyCollection<IRole> GetRoles()
		{
			lock (this)
			{
				if (IsExist == false)
					throw new ObjectDoesNotExistException(nameof(GetRoles));
				return member.Roles.Select(s => baseServer.GetRole(s.Id)).ToArray();
			}
		}

		/// <inheritdoc/>
		public Task GrantRoleAsync(IRole role)
		{
			lock (this)
			{
				if (IsExist == false)
					throw new ObjectDoesNotExistException(nameof(GrantRoleAsync));
				return BaseServer.SourceClient.DoSafeOperationAsync(() => member.GrantRoleAsync(((Role)role).BaseRole));
			}
		}

		/// <inheritdoc/>
		public Task RevokeRoleAsync(IRole role)
		{
			lock (this)
			{
				if (IsExist == false)
					throw new ObjectDoesNotExistException(nameof(RevokeRoleAsync));
				return BaseServer.SourceClient.DoSafeOperationAsync(() => member.RevokeRoleAsync(((Role)role).BaseRole));
			}
		}

		/// <inheritdoc/>
		public bool HasPermissionIn(Permissions permissions, IChannel channel) =>
			member.PermissionsIn(((Channel)channel).BaseChannel).GetAbstract().HasFlag(permissions);

		internal void ModifyInternal(DiscordMember member)
		{
			if (this.member.Id != member.Id)
				throw new InvalidOperationException("This operation can't modify id of model");

			lock(this)
			{
				this.member = member;
				base.ModifyInternal(member);
			}
		}


		internal Task SendDirectMessageAsyncInternal(MessageSendModel model)
		{
			lock (this)
			{
				return BaseServer.SourceClient.DoSafeOperationAsync(async () =>
				{
					var channel = await member.CreateDmChannelAsync();
					await channel.SendMessageAsync(MessageConverter.ConvertUp(model));
				}, new(DSharp.Client.UserName, Id, base.UserName));
			}
		}
	}
}