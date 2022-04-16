using CGZBot3.Entities.Message;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class Message : IMessage, IDisposable
	{
		private DiscordMessage message;
		private readonly TextChannel owner;
		private Lazy<MessageInteractionDispatcher> mid;
		private readonly MessageConverter converter = new();


		public TextChannel BaseChannel => owner;

		public MessageSendModel SendModel { get; private set; }

		public ulong Id => message.Id;

		public ITextChannel TextChannel => owner;

		public IMember Author { get; }

		public bool IsExist => owner.HasMessage(Id);


		public Message(DiscordMessage message, TextChannel owner, MessageSendModel sendModel)
		{
			this.message = message;
			this.owner = owner;
			SendModel = sendModel;
			Author = owner.Server.GetMember(message.Author.Id);
			mid = new Lazy<MessageInteractionDispatcher>(() => new MessageInteractionDispatcher(this));
		}

		~Message()
		{
			Dispose();
		}


		public bool Equals(IMessage? other) => other is Message msg && msg.Id == Id;

		public Task DeleteAsync() => message.DeleteAsync();

		public IInteractionDispatcher GetInteractionDispatcher() => mid.Value;

		public void Dispose()
		{
			if (mid.IsValueCreated) mid.Value.Dispose();
			GC.SuppressFinalize(this);
		}

		public async Task ModifyAsync(MessageSendModel sendModel, bool resetDispatcher)
		{
			if (resetDispatcher) ResetInteractionDispatcher();
			message = await message.ModifyAsync(converter.ConvertUp(SendModel = sendModel));
		}

		public void ResetInteractionDispatcher()
		{
			if (mid.IsValueCreated)
			{
				mid.Value.Dispose();
				mid = new Lazy<MessageInteractionDispatcher>(() => new MessageInteractionDispatcher(this));
			}
		}
	}
}
