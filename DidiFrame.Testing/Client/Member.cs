using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using System.Runtime.CompilerServices;

namespace DidiFrame.Testing.Client
{
	public class Member : User, IMember, IServerDeletable
	{
		private readonly Permissions permissions;
		private readonly ICollection<Role> roles = new HashSet<Role>();


		public Member(Server server, User baseUser, Permissions permissions) : base(server.BaseClient, baseUser.UserName, baseUser.IsBot)
		{
			Server = server;
			this.permissions = permissions;
			Id = baseUser.Id;
		}


		public IServer Server { get; }

		public ICollection<Role> Roles => GetIfExist(roles);

		public bool IsExist { get; private set; } = true;

		public bool Equals(IServerEntity? other) => other is Member member && IsExist && member.IsExist && member.Id == Id;


		public IReadOnlyCollection<IRole> GetRoles() => (IReadOnlyCollection<IRole>)GetIfExist(roles);

		public Task GrantRoleAsync(IRole role)
		{
			GetIfExist(roles).Add((Role)role);
			return Task.CompletedTask;
		}

		public bool HasPermissionIn(Permissions permissions, IChannel channel) => GetIfExist(this.permissions).HasFlag(permissions);

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