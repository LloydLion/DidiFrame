namespace DidiFrame.Clients
{
	public interface ITextChannelThread : ITextThreadBase
	{
		public new ITextChannel Parent { get; }
	}
}
