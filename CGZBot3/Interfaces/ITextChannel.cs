using CGZBot3.Entities.Message;

namespace CGZBot3.Interfaces
{
	public interface ITextChannel : IChannel
	{
		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel);

		public Task<IReadOnlyCollection<IMessage>> GetMessagesAsync(int count = -1);
	}
}