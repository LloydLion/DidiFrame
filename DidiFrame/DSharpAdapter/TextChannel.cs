using DidiFrame.Entities.Message;
using DidiFrame.Utils;
using DSharpPlus.Entities;

namespace DidiFrame.DSharpAdapter
{
	internal class TextChannel : Channel, ITextChannel
	{
		public const int MessagesLimit = 5;
		public const int MessagesIdLimit = 70;


		private readonly DiscordChannel channel;
		private readonly Server server;
		private readonly List<Message> messages = new();
		private readonly List<ulong> ids = new();
		private static readonly ThreadLocker<TextChannel> listLocker = new();


		public event MessageSentEventHandler? MessageSent;

		public event MessageDeletedEventHandler? MessageDeleted;


		public Server BaseServer => server;

		public IReadOnlyList<Message> Messages => messages;


		public TextChannel(DiscordChannel channel, Server server) : base(channel, server)
		{
			if(channel.Type.GetAbstract() != ChannelType.TextCompatible)
			{
				throw new InvalidOperationException("Channel must be text");
			}

			this.channel = channel;
			this.server = server;

			foreach (var item in channel.GetMessagesAsync(MessagesIdLimit).Result)
				ids.Add(item.Id);
		}

		public IMessage GetMessage(ulong id)
		{
			if (HasMessage(id) == false)
				throw new ArgumentException("No such message with same id", nameof(id));

			var msg = messages.SingleOrDefault(s => s.Id == id);
			if (msg is not null) return msg;
			else
			{
				//If not exist throw
				var discord = channel.GetMessageAsync(id).Result;
				return new Message(discord, this, MessageConverter.ConvertDown(discord));
			}
		}

		public IReadOnlyList<IMessage> GetMessages(int count = -1)
		{
			if (count == -1) return messages;
			return messages.AsEnumerable().Reverse().Take(Math.Min(count, messages.Count)).ToArray();
		}

		public bool HasMessage(ulong id) => ids.Contains(id);


		public async Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			var builder = MessageConverter.ConvertUp(messageSendModel);

			var msg = await server.SourceClient.DoSafeOperationAsync(async () =>
				new Message(owner: this, sendModel: messageSendModel, message: await channel.SendMessageAsync(builder)), new(Client.ChannelName, Id, Name));

			using (listLocker.Lock(this))
			{
				messages.Add(msg);
				if (messages.Count > MessagesLimit) messages.RemoveRange(0, messages.Count - MessagesLimit);

				ids.Add(msg.Id);
				if (ids.Count > MessagesIdLimit) ids.RemoveRange(0, ids.Count - MessagesIdLimit);
			}

			return msg;
		}

		public void OnMessageCreate(DiscordMessage message)
		{
			//Bot's messages already in list
			if (message.Author.Id == server.Client.SelfAccount.Id) return;

			var model = new Message(message, this, MessageConverter.ConvertDown(message));

			using (listLocker.Lock(this))
			{
				messages.Add(model);
				if(messages.Count > MessagesLimit) messages.RemoveRange(0, messages.Count - MessagesLimit);

				ids.Add(model.Id);
				if(ids.Count > MessagesIdLimit) ids.RemoveRange(0, ids.Count - MessagesIdLimit);
			}

			//Bot messages will be ignored
			MessageSent?.Invoke(server.Client, model);
			server.SourceClient.OnMessageCreated(model);
		}

		public void OnMessageDelete(DiscordMessage message)
		{
			ids.Remove(message.Id);

			var sor = messages.SingleOrDefault(s => s.Id == message.Id);
			if (sor is null) return;

			using (listLocker.Lock(this))
			{
				messages.Remove(sor);
				sor.Dispose();
			}

			//Bot messages will be ignored
			MessageDeleted?.Invoke(server.Client, sor);
			server.SourceClient.OnMessageDeleted(sor);
		}
	}
}
