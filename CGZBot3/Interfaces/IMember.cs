using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Interfaces
{
	internal interface IMember : IServerEntity, IUser
	{
		public Task<IReadOnlyCollection<IRole>> GetRolesAsync();

		public Task GrantRoleAsync(IRole role);

		public Task RevokeRoleAsync(IRole role);

		public bool HasPermissionIn(Permissions permissions, IChannel channel);
	}
}
