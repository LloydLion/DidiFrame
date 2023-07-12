namespace DidiFrame.Clients
{
	public interface IMember : IUser, IPermissionSubject
	{
		public string Nickname { get; }


		public IReadOnlyList<IRole> ListRoles();

		public ValueTask GrantRoleAsync(IRole role);

		public ValueTask RevokeRoleAsync(IRole role);
	}
}
