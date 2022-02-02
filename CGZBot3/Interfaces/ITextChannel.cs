namespace CGZBot3.Interfaces
{
	internal interface ITextChannel : IChannel
	{
		public IMessage SendMessage(IMessageSendModel messageSendModel);
	}
}