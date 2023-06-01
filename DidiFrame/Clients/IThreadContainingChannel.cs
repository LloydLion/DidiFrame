namespace DidiFrame.Clients
{
	public interface IThreadContainingChannel<out TThread> : IChannel where TThread : IThreadChannel
	{
		public IReadOnlyCollection<TThread> ListThreads();
	}
}
