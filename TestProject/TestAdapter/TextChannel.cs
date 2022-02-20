using CGZBot3.Entities;
using CGZBot3.Interfaces;
using System.Collections.Generic;
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


		public IList<Message> Messages { get; } = new List<Message>();
	}
}
