namespace DidiFrame.Clients
{
	public interface ITextChannelBase : IChannel, IMessageContainer
	{
		public new ValueTask<IServerMessage> SendMessageAsync(MessageSendModel message);

		public new ValueTask<IReadOnlyList<IServerMessage>> ListMessagesAsync(int limit = 25);

		public new ValueTask<IServerMessage> GetMessageAsync(ulong id);
	}
}
