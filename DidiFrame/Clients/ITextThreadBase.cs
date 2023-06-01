namespace DidiFrame.Clients
{
	public interface ITextThreadBase : IThreadChannel, ITextChannelBase
	{
		public new IThreadContainingChannel<ITextThreadBase> Parent { get; }
	}
}
