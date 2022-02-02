namespace CGZBot3.Interfaces
{
	internal interface ITextChannel : IChannel
	{
		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel);
	}
}