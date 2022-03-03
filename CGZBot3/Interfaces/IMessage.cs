using CGZBot3.Entities.Message;

namespace CGZBot3.Interfaces
{
	public interface IMessage : IEquatable<IMessage>
	{
		public MessageSendModel SendModel { get; }

		public string Content { get { return SendModel.Content; } }

		public ulong Id { get; }

		public ITextChannel TextChannel { get; }

		public IMember Author { get; }


		Task DeleteAsync();
	}
}