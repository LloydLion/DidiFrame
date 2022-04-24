using DidiFrame.Entities.Message;

namespace DidiFrame.Interfaces
{
	public interface IMessage : IEquatable<IMessage>
	{
		public MessageSendModel SendModel { get; }

		public string? Content { get { return SendModel.Content; } }

		public ulong Id { get; }

		public ITextChannel TextChannel { get; }

		public IMember Author { get; }

		public bool IsExist { get; }


		public Task DeleteAsync();

		public IInteractionDispatcher GetInteractionDispatcher();

		public Task ModifyAsync(MessageSendModel sendModel, bool resetDispatcher);

		public void ResetInteractionDispatcher();
	}
}