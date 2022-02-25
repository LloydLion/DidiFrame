using CGZBot3.Entities;
using CGZBot3.Entities.Message;
using CGZBot3.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.TestAdapter
{
	internal class TextChannel : Channel, ITextChannel
	{
		public TextChannel(string name, ChannelCategory category) : base(name, category) { }


		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			var msg = new Message(messageSendModel, this);
			Messages.Add(msg);
			return Task.FromResult((IMessage)msg);
		}

		public Task<IReadOnlyCollection<IMessage>> GetMessagesAsync(int count = -1)
		{
			return Task.FromResult((IReadOnlyCollection<IMessage>)(count == -1 ? Messages : Messages.Take(count).ToArray()));
		}


		public IList<Message> Messages { get; } = new List<Message>();
	}
}
