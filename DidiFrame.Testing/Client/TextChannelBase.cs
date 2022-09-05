using DidiFrame.Entities.Message;
using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	public class TextChannelBase : Channel, ITextChannelBase
	{
		internal TextChannelBase(string name, ChannelCategory category) : base(name, category) { }


		public event MessageSentEventHandler? MessageSent;

		public event MessageDeletedEventHandler? MessageDeleted;


		private readonly List<Message> msgs = new();


		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			var message = AddMessage((Member)BaseServer.GetMember(BaseServer.Client.SelfAccount), messageSendModel);
			return Task.FromResult((IMessage)message);
		}

		public IReadOnlyList<IMessage> GetMessages(int count = -1)
		{
			return count == -1 ? GetIfExist(msgs) : GetIfExist(msgs).Take(count).ToArray();
		}

		public IMessage GetMessage(ulong id)
		{
			return GetIfExist(msgs).Single(s => s.Id == id);
		}

		public bool HasMessage(ulong id)
		{
			return GetIfExist(msgs).Any(s => s.Id == id);
		}


		public Message AddMessage(Member sender, MessageSendModel sendModel)
		{
			var message = new Message(sendModel, this, sender);
			GetIfExist(msgs).Add(message);

			try { MessageSent?.Invoke(BaseCategory.BaseServer.BaseClient, message, false); }
			catch (Exception) { /*No logging*/ }
			BaseServer.OnMessageCreated(message, false);

			return message;
		}

		public void EditMessage(Message message, MessageSendModel sendModel)
		{
			message.ModifyInternal(sendModel);

			try { MessageSent?.Invoke(BaseCategory.BaseServer.BaseClient, message, true); }
			catch (Exception) { /*No logging*/ }
			BaseServer.OnMessageCreated(message, true);
		}

		public void DeleteMessage(Message message)
		{
			GetIfExist(msgs).Remove(message);

			try { MessageDeleted?.Invoke(BaseCategory.BaseServer.BaseClient, this, message.Id); }
			catch (Exception) { /*No logging*/ }
			BaseServer.OnMessageDeleted(this, message.Id);
		}
	}
}
