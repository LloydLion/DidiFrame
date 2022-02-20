namespace CGZBot3.Interfaces
{
	public interface ITextChannel : IChannel
	{
		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel);
	}
}