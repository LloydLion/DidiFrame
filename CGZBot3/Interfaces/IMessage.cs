namespace CGZBot3.Interfaces
{
	internal interface IMessage : IEquatable<IMessage>
	{
		public MessageSendModel SendModel { get; }

		public string Content { get { return SendModel.Content; } }

		public string Id { get; }

		public ITextChannel TextChannel { get; }
	}
}