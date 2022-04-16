using CGZBot3.Data.Model;
using CGZBot3.Entities.Message;

namespace CGZBot3.Utils
{
	public class MessageAliveHolder : IDisposable
	{
		private readonly Func<MessageSendModel> messageCreator;
		private readonly Action<IMessage> messageWoker;
		private readonly static ThreadLocker<MessageAliveHolder> threadLocker = new();
		private readonly Model model;
		private readonly bool active;


		public MessageAliveHolder(Model model, bool active, Func<MessageSendModel> messageCreator, Action<IMessage> messageWorker)
		{
			this.model = model;
			this.messageCreator = messageCreator;
			this.messageWoker = messageWorker;
			this.active = active;
			if (active) model.Channel.MessageDeleted += OnMessageDeleted;
		}


		public event Action<IMessage>? AutoMessageCreated;


		public IMessage Message
		{
			get
			{
				using (threadLocker.Lock(this))
				{
					if (Channel.HasMessage(PossibleMessageId) == false) CheckAsync().Wait();
					return Channel.GetMessage(PossibleMessageId);
				}
			}
		}

		public void ProcessMessage()
		{
			messageWoker(Channel.GetMessage(PossibleMessageId));
		}

		public ulong PossibleMessageId => model.PossibleMessageId;

		public ITextChannel Channel => model.Channel;

		public bool ActiveMode => active;

		public bool IsExist => Channel.HasMessage(PossibleMessageId);


		public Task DeleteAsync()
		{
			using (threadLocker.Lock(this))
			{
				if (Channel.HasMessage(PossibleMessageId)) return Channel.GetMessage(PossibleMessageId).DeleteAsync();
				else return Task.CompletedTask;
			}
		}

		public async Task Update()
		{
			using (threadLocker.Lock(this))
			{
				if (Channel.HasMessage(PossibleMessageId) == false)
					model.PossibleMessageId = (await SendMessageAsync()).Id;
				else
				{
					var message = Channel.GetMessage(model.PossibleMessageId);
					await message.ModifyAsync(messageCreator(), true);
					messageWoker(message);
				}
			}
		}

		private async Task<IMessage> SendMessageAsync()
		{
			var send = messageCreator();
			var msg = await Channel.SendMessageAsync(send);
			messageWoker(msg);
			return msg;
		}

		private void OnMessageDeleted(IClient _, IMessage message)
		{
			if (PossibleMessageId != message.Id) return;

			using (threadLocker.Lock(this))
			{
				CheckAsync().Wait();
			}
		}

		public async void Dispose()
		{
			if(active) Channel.MessageDeleted -= OnMessageDeleted;
			GC.SuppressFinalize(this);
			await DeleteAsync();
		}

		public async Task CheckAsync()
		{
			if (Channel.HasMessage(PossibleMessageId) == false)
			{
				var message = await SendMessageAsync();
				model.PossibleMessageId = message.Id;
				AutoMessageCreated?.Invoke(message);
			}
		}


		public class Model
		{
			public Model(ulong possibleMessageId, ITextChannel channel)
			{
				PossibleMessageId = possibleMessageId;
				Channel = channel;
			}

			public Model(ITextChannel channel)
			{
				Channel = channel;
			}


			[ConstructorAssignableProperty(0, "possibleMessageId")]
			public ulong PossibleMessageId { get; set; }

			[ConstructorAssignableProperty(1, "channel")]
			public ITextChannel Channel { get; }
		}
	}
}
