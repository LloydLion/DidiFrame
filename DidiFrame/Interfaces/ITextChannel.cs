namespace DidiFrame.Interfaces
{
	public interface ITextChannel : ITextChannelBase
	{
		public IReadOnlyCollection<ITextThread> GetThreads();
	}
}
