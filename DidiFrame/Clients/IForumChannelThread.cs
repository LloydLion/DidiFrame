namespace DidiFrame.Clients
{
	public interface IForumChannelThread : ITextThreadBase
	{
		public new IForumChannel Parent { get; }
	}
}