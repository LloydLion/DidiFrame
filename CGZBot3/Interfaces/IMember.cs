using CGZBot3.Entities.Message;

namespace CGZBot3.Interfaces
{
	public interface IMember : IServerEntity, IUser
	{
		public bool IsBot { get; }


		public Task<IReadOnlyCollection<IRole>> GetRolesAsync();

		public Task GrantRoleAsync(IRole role);

		public Task RevokeRoleAsync(IRole role);

		public bool HasPermissionIn(Permissions permissions, IChannel channel);

		Task SendDirectMessageAsync(MessageSendModel model);
	}
}
