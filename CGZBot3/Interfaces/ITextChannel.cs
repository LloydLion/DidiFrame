using CGZBot3.Entities.Message;

namespace CGZBot3.Interfaces
{
	public interface ITextChannel : IChannel
	{
		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel);

		public IReadOnlyList<IMessage> GetMessages(int count = -1);

		public IMessage GetMessage(ulong id);


		public event MessageSentEventHandler? MessageSent;
	}
}