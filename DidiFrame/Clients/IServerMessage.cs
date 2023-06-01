namespace DidiFrame.Clients
{
	public interface IServerMessage : IMessage
	{
		public ITextChannelBase Channel { get; }

		public new IMember Author { get; }
	}
}