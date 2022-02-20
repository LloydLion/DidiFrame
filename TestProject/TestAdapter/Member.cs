using CGZBot3.Entities;
using CGZBot3.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestProject.TestAdapter
{
	internal class Member : User, IMember
	{
		private readonly Permissions permissions;


		public Member(Server server, User baseUser, Permissions permissions) : base(server.BaseClient, baseUser.UserName)
		{
			Server = server;
			this.permissions = permissions;
			Id = baseUser.Id;
		}

		public IServer Server { get; }

		public ICollection<Role> Roles { get; } = new HashSet<Role>();


		public bool Equals(IServerEntity? other) => other is Member member && member.Id == Id;

		public Task<IReadOnlyCollection<IRole>> GetRolesAsync()
		{
			return Task.FromResult((IReadOnlyCollection<IRole>)Roles);
		}

		public Task GrantRoleAsync(IRole role)
		{
			Roles.Add((Role)role);
			return Task.CompletedTask;
		}

		public bool HasPermissionIn(Permissions permissions, IChannel channel)
		{
			return this.permissions.HasFlag(permissions);
		}

		public Task RevokeRoleAsync(IRole role)
		{
			Roles.Add((Role)role);
			return Task.CompletedTask;
		}
	}
}