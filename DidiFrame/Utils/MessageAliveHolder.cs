using DidiFrame.Data.Model;

namespace DidiFrame.Utils
{
	/// <summary>
	/// Container for discord message
	/// </summary>
	public class MessageAliveHolder : IDisposable
	{
		private readonly Func<MessageSendModel> messageCreator;
		private readonly Action<IMessage> messageWoker;
		private readonly static ThreadLocker<MessageAliveHolder> threadLocker = new();
		private readonly Model model;
		private readonly bool active;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.MessageAliveHolder using base serialization model
		/// </summary>
		/// <param name="model">Base model for DidiFrame.Utils.MessageAliveHolder object</param>
		/// <param name="active">If need display message always unless only by request</param>
		/// <param name="messageCreator">Creator of message that can create msg's send model by request</param>
		/// <param name="messageWorker">Post propessor for created message</param>
		public MessageAliveHolder(Model model, bool active, Func<MessageSendModel> messageCreator, Action<IMessage> messageWorker)
		{
			this.model = model;
			this.messageCreator = messageCreator;
			this.messageWoker = messageWorker;
			this.active = active;
			if (active) model.Channel.MessageDeleted += OnMessageDeleted;
		}


		/// <summary>
		/// Event that fires when message was deleted and automaticly regenerated
		/// </summary>
		public event Action<IMessage>? AutoMessageCreated;


		/// <summary>
		/// Provides safe accsess to message
		/// </summary>
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

		/// <summary>
		/// Processes message through given message worker
		/// </summary>
		public void ProcessMessage()
		{
			messageWoker(Channel.GetMessage(PossibleMessageId));
		}

		/// <summary>
		/// Id of message that can exist but can dotn't exist
		/// </summary>
		public ulong PossibleMessageId => model.PossibleMessageId;

		/// <summary>
		/// Target channel
		/// </summary>
		public ITextChannel Channel => model.Channel;

		/// <summary>
		/// If need display message always unless only by request
		/// </summary>
		public bool ActiveMode => active;

		/// <summary>
		/// If message still exist
		/// </summary>
		public bool IsExist => Channel.HasMessage(PossibleMessageId);


		/// <summary>
		/// Deletes message async
		/// </summary>
		/// <returns>Wait task</returns>
		public Task DeleteAsync()
		{
			using (threadLocker.Lock(this))
			{
				if (Channel.HasMessage(PossibleMessageId)) return Channel.GetMessage(PossibleMessageId).DeleteAsync();
				else return Task.CompletedTask;
			}
		}

		/// <summary>
		/// Modifies message using new send model from messages creator
		/// </summary>
		/// <returns>Wait task</returns>
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

		/// <summary>
		/// Checks message existance and if dotn't exist creates, calls AutoMessageCreated event
		/// </summary>
		/// <returns>Wait task</returns>
		public async Task CheckAsync()
		{
			if (Channel.HasMessage(PossibleMessageId) == false)
			{
				var message = await SendMessageAsync();
				model.PossibleMessageId = message.Id;
				AutoMessageCreated?.Invoke(message);
			}
		}


		/// <summary>
		/// Serialization model of DidiFrame.Utils.MessageAliveHolder
		/// </summary>
		public class Model
		{
			/// <summary>
			/// Creates DidiFrame.Utils.MessageAliveHolder.Model using possible id and channel
			/// </summary>
			/// <param name="possibleMessageId">Possible message id</param>
			/// <param name="channel">Channel where need to create message</param>
			public Model(ulong possibleMessageId, ITextChannel channel)
			{
				PossibleMessageId = possibleMessageId;
				Channel = channel;
			}

			/// <summary>
			/// Creates DidiFrame.Utils.MessageAliveHolder.Model using channel
			/// </summary>
			/// <param name="channel">Channel where need to create message</param>
			public Model(ITextChannel channel)
			{
				Channel = channel;
			}


			/// <summary>
			/// Id of message that can exist but can dotn't exist
			/// </summary>
			[ConstructorAssignableProperty(0, "possibleMessageId")]
			public ulong PossibleMessageId { get; set; }

			/// <summary>
			/// Channel where need to create message
			/// </summary>
			[ConstructorAssignableProperty(1, "channel")]
			public ITextChannel Channel { get; }
		}
	}
}
