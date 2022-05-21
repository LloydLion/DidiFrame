namespace DidiFrame.Interfaces
{
	/// <summary>
	/// Represents a discord member - user on server
	/// </summary>
	public interface IMember : IServerEntity, IUser
	{
		/// <summary>
		/// Gets a list of members's roles
		/// </summary>
		/// <returns>Collection of roles</returns>
		public IReadOnlyCollection<IRole> GetRoles();

		/// <summary>
		/// Grants role to member async. Be careful with it, discord can disallow this operation
		/// </summary>
		/// <param name="role">Role object</param>
		/// <returns>Wait task</returns>
		public Task GrantRoleAsync(IRole role);

		/// <summary>
		/// Revokes role to member async. Be careful with it, discord can disallow this operation
		/// </summary>
		/// <param name="role">Role object</param>
		/// <returns>Wait task</returns>
		public Task RevokeRoleAsync(IRole role);

		/// <summary>
		/// Checks member's permissions in channel
		/// </summary>
		/// <param name="permissions">Permissions to check</param>
		/// <param name="channel">Target channel</param>
		/// <returns>Has or hasn't</returns>
		public bool HasPermissionIn(Permissions permissions, IChannel channel);
	}
}
