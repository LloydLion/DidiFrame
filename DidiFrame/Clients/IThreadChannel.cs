namespace DidiFrame.Clients
{
	public interface IThreadChannel : IChannel
	{
		public IThreadContainingChannel<IThreadChannel> Parent { get; }


		public IReadOnlyCollection<IMember> ListMembers();
	}
}
