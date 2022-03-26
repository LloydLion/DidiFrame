using CGZBot3.Entities.Message;
using CGZBot3.Entities.Message.Components;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Text;

namespace CGZBot3.DSharpAdapter
{
	internal class TextChannel : Channel, ITextChannel
	{
		private readonly DiscordChannel channel;
		private readonly Server server;
		private readonly MessageConverter converter;


		public Server BaseServer => server;


		public TextChannel(DiscordChannel channel, Server server) : base(channel, server)
		{
			if(channel.Type.GetAbstract() != Entities.ChannelType.TextCompatible )
			{
				throw new InvalidOperationException("Channel must be text");
			}

			this.channel = channel;
			this.server = server;
			converter = new();
		}

		public IMessage GetMessage(ulong id)
		{
			var msg = channel.GetMessageAsync(id).Result;
			var model = converter.ConvertDown(msg);

			return new Message(msg, this, model);
		}

		public IReadOnlyList<IMessage> GetMessages(int count = -1)
		{
			return channel.GetMessagesAsync(count == -1 ? 1000 : count).Result.Select(s => new Message(s, this, new MessageSendModel(s.Content))).ToArray();
		}

		public async Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			var builder = await converter.ConvertUpAsync(messageSendModel);

			var msg = new Message(owner: this, sendModel: messageSendModel, message: await channel.SendMessageAsync(builder));
			return msg;
		}
	}
}
