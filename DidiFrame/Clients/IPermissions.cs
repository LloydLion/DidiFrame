namespace DidiFrame.Clients
{
	public interface IPermissions
	{
		public ValueTask GrantPermissionsAsync(Permissions permissions, IPermissionSubject subject);

		public ValueTask ResetPermissionsAsync(Permissions permissions, IPermissionSubject subject);

		public ValueTask RevokePermissionsAsync(Permissions permissions, IPermissionSubject subject);

		public ValueTask OverridePermissionsAsync(PermissionsOverride permissions, IPermissionSubject subject);

		public Permissions CalculatePermissionsFor(IMember member);

		public IReadOnlyCollection<IPermissionSubject> ListSubjects();

		public PermissionsOverride GetOverrideFor(IPermissionSubject subject);
	}
}
