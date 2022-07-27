using DidiFrame.Entities.Message;
using DidiFrame.Interfaces;

namespace DidiFrame.Testing.Client
{
	public class Message : IMessage
	{
		private Lazy<MessageInteractionDispatcher> mid;


		public Message(MessageSendModel sendModel, TextChannelBase baseTextChannel, Member author)
		{
			SendModel = sendModel;
			BaseTextChannel = baseTextChannel;
			Id = baseTextChannel.BaseServer.BaseClient.GenerateId();
			Author = author;
			mid = new Lazy<MessageInteractionDispatcher>(() => new MessageInteractionDispatcher(this));
		}


		public MessageSendModel SendModel { get; private set; }

		public ulong Id { get; }

		public ITextChannelBase TextChannel => BaseTextChannel;

		public TextChannelBase BaseTextChannel { get; }

		public IMember Author { get; }

		public bool IsExist => BaseTextChannel.HasMessage(Id);


		public Task DeleteAsync()
		{
			BaseTextChannel.DeleteMessage(this);
			return Task.CompletedTask;
		}

		public bool Equals(IMessage? other) => other is Message message && message.Id == Id;

		public IInteractionDispatcher GetInteractionDispatcher() => GetBaseInteractionDispatcher();

		public MessageInteractionDispatcher GetBaseInteractionDispatcher() => mid.Value;

		public Task ModifyAsync(MessageSendModel sendModel, bool resetDispatcher)
		{
			if (resetDispatcher) ResetInteractionDispatcher();
			SendModel = sendModel;
			return Task.CompletedTask;
		}

		public void ResetInteractionDispatcher()
		{
			if (mid.IsValueCreated) mid = new Lazy<MessageInteractionDispatcher>(() => new MessageInteractionDispatcher(this));
		}
	}
}
