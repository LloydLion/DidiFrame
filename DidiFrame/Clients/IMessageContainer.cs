namespace DidiFrame.Clients
{
	public interface IMessageContainer : IDiscordObject
	{
		public ValueTask<IServerMessage> SendMessageAsync(MessageSendModel message);

		public IReadOnlyCollection<IServerMessage> ListMessages(int limit = 25);

		public IMessage GetMessage(ulong possibleMessageId);
	}
}
