namespace DidiFrame.Clients
{
	public interface IChannel : ICategoryItem
	{
		public IReadOnlyList<IMember> ListMembers();

		public IChannelPermissions ManagePermissions();
	}
}
