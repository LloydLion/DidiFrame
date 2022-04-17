using CGZBot3.Entities.Message;

namespace CGZBot3.Interfaces
{
	public interface IMember : IServerEntity, IUser
	{
		public IReadOnlyCollection<IRole> GetRoles();

		public Task GrantRoleAsync(IRole role);

		public Task RevokeRoleAsync(IRole role);

		public bool HasPermissionIn(Permissions permissions, IChannel channel);

		Task SendDirectMessageAsync(MessageSendModel model);
	}
}
