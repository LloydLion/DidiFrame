using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Clients;
using System.Runtime.CompilerServices;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test IMember implementation
	/// </summary>
	public class Member : User, IMember, IServerDeletable
	{
		private readonly Permissions permissions;
		private readonly ICollection<Role> roles = new HashSet<Role>();


		internal Member(Server server, User baseUser, Permissions permissions) : base(server.BaseClient, baseUser.UserName, baseUser.IsBot, baseUser.Id)
		{
			Server = server;
			this.permissions = permissions;
		}

		internal Member(Server server, string userName, bool isBot, Permissions permissions) : base(server.BaseClient, userName, isBot)
		{
			Server = server;
			this.permissions = permissions;
		}


		/// <inheritdoc/>
		public IServer Server { get; }

		/// <inheritdoc/>
		public ICollection<Role> Roles => GetIfExist(roles);

		/// <inheritdoc/>
		public bool IsExist { get; private set; } = true;

		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => Equals(other as IMember);

		/// <inheritdoc/>
		public bool Equals(IMember? other) => base.Equals(other);

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as IMember);

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();

		/// <inheritdoc/>
		public IReadOnlyCollection<IRole> GetRoles() => (IReadOnlyCollection<IRole>)GetIfExist(roles);

		/// <inheritdoc/>
		public Task GrantRoleAsync(IRole role)
		{
			GetIfExist(roles).Add((Role)role);
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public bool HasPermissionIn(Permissions permissions, IChannel channel) => GetIfExist(this.permissions).HasFlag(permissions);

		/// <inheritdoc/>
		public Task RevokeRoleAsync(IRole role)
		{
			GetIfExist(roles).Remove((Role)role);
			return Task.CompletedTask;
		}

		void IServerDeletable.DeleteInternal()
		{
			IsExist = false;
		}

		private TValue GetIfExist<TValue>(TValue value, [CallerMemberName] string nameOfCaller = "")
		{
			if (IsExist == false)
				throw new ObjectDoesNotExistException(nameOfCaller);
			else return value;
		}
	}
}