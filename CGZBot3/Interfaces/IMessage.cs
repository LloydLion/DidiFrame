namespace CGZBot3.Interfaces
{
	public interface IMessage
	{
		public IMessageSendModel SendModel { get; }

		public string Content { get { return SendModel.Content; } }

		public string Id { get; }
	}
}