using DidiFrame.Entities.Message;

namespace DidiFrame.Interfaces
{
	public interface IMember : IServerEntity, IUser
	{
		public IReadOnlyCollection<IRole> GetRoles();

		public Task GrantRoleAsync(IRole role);

		public Task RevokeRoleAsync(IRole role);

		public bool HasPermissionIn(Permissions permissions, IChannel channel);
	}
}
