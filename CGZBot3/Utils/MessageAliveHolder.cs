using CGZBot3.Data.Model;
using CGZBot3.Entities.Message;

namespace CGZBot3.Utils
{
	public class MessageAliveHolder
	{
		public MessageAliveHolder(ulong possibleMessageId, MessageSendModel sendModel, ITextChannel channel)
		{
			PossibleMessageId = possibleMessageId;
			SendModel = sendModel;
			Channel = channel;
		}

		public MessageAliveHolder(IMessage message)
		{
			if (message.IsExist == false) throw new InvalidOperationException("Message has not exist");

			Channel = message.TextChannel;
			PossibleMessageId = message.Id;
			SendModel = message.SendModel;
		}


		public IMessage Message
		{
			get 
			{
				var toSend = !Channel.HasMessage(PossibleMessageId);
				if (toSend)
				{
					var message = Channel.SendMessageAsync(SendModel).Result;
					PossibleMessageId = message.Id;
					return message;
				}
				else return Channel.GetMessage(PossibleMessageId);
			}
		}

		[ConstructorAssignableProperty(0, "possibleMessageId")]
		public ulong PossibleMessageId { get; private set; }

		[ConstructorAssignableProperty(1, "sendModel")]
		public MessageSendModel SendModel { get; }

		[ConstructorAssignableProperty(2, "channel")]
		public ITextChannel Channel { get; }


		public Task DeleteAsync()
		{
			if (Channel.HasMessage(PossibleMessageId)) return Channel.GetMessage(PossibleMessageId).DeleteAsync();
			else return Task.CompletedTask;
		}
	}
}
