namespace DidiFrame.Clients
{
	public interface IMember : IUser, IPermissionSubject
	{
		public string Nickname { get; }
	}
}
