using CGZBot3.Entities.Message;
using CGZBot3.Utils;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class TextChannel : Channel, ITextChannel
	{
		public const int MessagesLimit = 50;


		private readonly DiscordChannel channel;
		private readonly Server server;
		private readonly MessageConverter converter;
		private readonly List<Message> messages = new();
		private static readonly ThreadLocker<TextChannel> listLocker = new();


		public Server BaseServer => server;

		public IReadOnlyList<Message> Messages => messages;


		public TextChannel(DiscordChannel channel, Server server) : base(channel, server)
		{
			if(channel.Type.GetAbstract() != ChannelType.TextCompatible )
			{
				throw new InvalidOperationException("Channel must be text");
			}

			this.channel = channel;
			this.server = server;
			converter = new();
		}

		public IMessage GetMessage(ulong id)
		{
			return messages.Single(s => s.Id == id);
		}

		public IReadOnlyList<IMessage> GetMessages(int count = -1)
		{
			if (count == -1) return messages;
			return messages.AsEnumerable().Reverse().Take(Math.Min(count, messages.Count)).ToArray();
		}

		public async Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			var builder = converter.ConvertUp(messageSendModel);

			var msg = new Message(owner: this, sendModel: messageSendModel, message: await channel.SendMessageAsync(builder));

			using (listLocker.Lock(this))
			{
				messages.Add(msg);
				if (messages.Count > MessagesLimit) messages.RemoveRange(0, messages.Count - MessagesLimit);
			}

			return msg;
		}

		public void OnMessageCreate(DiscordMessage message)
		{
			//Bot's messages already in list
			if (message.Author.Id == server.Client.SelfAccount.Id) return;

			var model = new Message(message, this, converter.ConvertDown(message));

			using (listLocker.Lock(this))
			{
				messages.Add(model);
				if(messages.Count > MessagesLimit) messages.RemoveRange(0, messages.Count - MessagesLimit);
			}
		}

		public void OnMessageDelete(DiscordMessage message)
		{
			var sor = messages.SingleOrDefault(s => s.Id == message.Id);
			if (sor is null) return;

			using (listLocker.Lock(this))
			{
				messages.Remove(sor);
				sor.Dispose();
			}
		}

		public void SetMessages(IReadOnlyList<DiscordMessage> newMessages)
		{
			using (listLocker.Lock(this))
			{
				var temp = messages.ToList();
				messages.Clear();

				foreach (var msg in newMessages)
				{
					var maybe = temp.SingleOrDefault(s => s.Id == msg.Id);
					if (maybe is not null)
					{
						messages.Add(maybe);
						temp.Remove(maybe);
					}
					else messages.Add(new Message(msg, this, converter.ConvertDown(msg)));
				}

				foreach (var item in temp) item.Dispose();
			}
		}
	}
}
