using CGZBot3.Entities.Message;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace CGZBot3.DSharpAdapter
{
	internal class Message : IMessage, IDisposable
	{
		private readonly DiscordMessage message;
		private readonly TextChannel owner;
		private Lazy<MessageInteractionDispatcher> mid;


		public TextChannel BaseChannel => owner;

		public MessageSendModel SendModel { get; }

		public ulong Id => message.Id;

		public ITextChannel TextChannel => owner;

		public IMember Author { get; }

		public bool IsExist
		{
			get
			{
				try
				{
					owner.GetMessage(Id);
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}


		public Message(DiscordMessage message, TextChannel owner, MessageSendModel sendModel)
		{
			this.message = message;
			this.owner = owner;
			SendModel = sendModel;
			Author = owner.Server.GetMember(message.Author.Id);
			mid = new Lazy<MessageInteractionDispatcher>(() => new MessageInteractionDispatcher(this));
		}


		public bool Equals(IMessage? other) => other is Message msg && msg.Id == Id;

		public Task DeleteAsync() => message.DeleteAsync();

		public IInteractionDispatcher GetInteractionDispatcher() => mid.Value;

		public void Dispose()
		{
			if (mid.IsValueCreated) mid.Value.Dispose();
		}
	}
}
