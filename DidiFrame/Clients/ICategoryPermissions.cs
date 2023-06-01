namespace DidiFrame.Clients
{
	public interface ICategoryPermissions : IPermissions
	{
		public ValueTask SyncPermissionsAsync();
	}
}
