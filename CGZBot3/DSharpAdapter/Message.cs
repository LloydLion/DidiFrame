using CGZBot3.Entities.Message;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace CGZBot3.DSharpAdapter
{
	internal class Message : IMessage
	{
		private readonly DiscordMessage message;
		private readonly TextChannel owner;


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
					owner.BaseChannel.GetMessageAsync(Id);
					return true;
				}
				catch (AggregateException ex)
				{
					if (ex.InnerException is NotFoundException) return false;
					else throw;
				}
			}
		}


		public Message(DiscordMessage message, TextChannel owner, MessageSendModel sendModel)
		{
			this.message = message;
			this.owner = owner;
			SendModel = sendModel;
			Author = owner.Server.GetMemberAsync(message.Author.Id).Result;
		}


		public bool Equals(IMessage? other) => other is Message msg && msg.Id == Id;

		public Task DeleteAsync() => message.DeleteAsync();
	}
}
