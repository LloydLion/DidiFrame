using CGZBot3.Entities;
using CGZBot3.Interfaces;

namespace TestProject.TestAdapter
{
	internal class Message : IMessage
	{
		public Message(MessageSendModel sendModel, TextChannel baseTextChannel)
		{
			SendModel = sendModel;
			BaseTextChannel = baseTextChannel;
			Id = baseTextChannel.BaseServer.BaseClient.GenerateId();
		}


		public MessageSendModel SendModel { get; }

		public ulong Id { get; }

		public ITextChannel TextChannel => BaseTextChannel;

		public TextChannel BaseTextChannel { get; }


		public bool Equals(IMessage? other) => other is Message message && message.Id == Id;
	}
}
