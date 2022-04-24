using DidiFrame.Entities.Message;

namespace DidiFrame.Interfaces
{
	public interface ITextChannel : IChannel
	{
		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel);

		public IReadOnlyList<IMessage> GetMessages(int count = -1);

		public IMessage GetMessage(ulong id);

		public bool HasMessage(ulong id);


		public event MessageSentEventHandler? MessageSent;

		public event MessageDeletedEventHandler? MessageDeleted;
	}
}