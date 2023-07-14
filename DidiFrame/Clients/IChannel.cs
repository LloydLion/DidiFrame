namespace DidiFrame.Clients
{
	public interface IChannel : ICategoryItem
	{
		public IChannelPermissions ManagePermissions();
	}
}
