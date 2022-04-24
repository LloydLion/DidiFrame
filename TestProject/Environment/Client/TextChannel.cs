using DidiFrame.Entities;
using DidiFrame.Entities.Message;
using DidiFrame.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.Environment.Client
{
	internal class TextChannel : Channel, ITextChannel
	{
		public TextChannel(string name, ChannelCategory category) : base(name, category) { }


		public event MessageSentEventHandler? MessageSent;
		public event MessageDeletedEventHandler? MessageDeleted;


		private readonly List<Message> msgs = new();


		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			var server = BaseCategory.BaseServer;
			var msg = new Message(messageSendModel, this, server.Members.Single(s => s.Id == server.Client.SelfAccount.Id));
			AddMessage(msg);
			return Task.FromResult((IMessage)msg);
		}

		public IReadOnlyList<IMessage> GetMessages(int count = -1)
		{
			return count == -1 ? msgs : msgs.Take(count).ToArray();
		}

		public IMessage GetMessage(ulong id)
		{
			return msgs.Single(s => s.Id == id);
		}

		public bool HasMessage(ulong id)
		{
			return msgs.Any(s => s.Id == id);
		}


		public void AddMessage(Message msg)
		{
			msgs.Add(msg);
			MessageSent?.Invoke(BaseCategory.BaseServer.BaseClient, msg);
			BaseCategory.BaseServer.BaseClient.OnMessageCreated(msg);
		}

		public void DeleteMessage(Message msg)
		{
			msgs.Remove(msg);
			MessageDeleted?.Invoke(BaseCategory.BaseServer.BaseClient, msg);
			BaseCategory.BaseServer.BaseClient.OnMessageDeleted(msg);
		}
	}
}
