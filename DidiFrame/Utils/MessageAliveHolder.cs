using DidiFrame.Data.Model;

namespace DidiFrame.Utils
{
	/// <summary>
	/// Container for discord message
	/// </summary>
	public class MessageAliveHolder : IDisposable
	{
		private readonly Func<MessageSendModel> messageCreator;
		private readonly Action<IMessage> messageWorker;
		private readonly static ThreadLocker<MessageAliveHolder> threadLocker = new();
		private readonly IObjectController<Model> model;
		private readonly bool active;
		private bool targetMessageState = true;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.MessageAliveHolder using base serialization model
		/// </summary>
		/// <param name="model">Base model for DidiFrame.Utils.MessageAliveHolder object</param>
		/// <param name="active">If need display message always unless only by request</param>
		/// <param name="messageCreator">Creator of message that can create msg's send model by request</param>
		/// <param name="messageWorker">Post propessor for created message</param>
		public MessageAliveHolder(IObjectController<Model> model, bool active, Func<MessageSendModel> messageCreator, Action<IMessage> messageWorker)
		{
			using var instance = model.Open();
			this.model = model;
			this.messageCreator = messageCreator;
			this.messageWorker = messageWorker;
			this.active = active;
			if (active) instance.Object.Channel.MessageDeleted += OnMessageDeleted;
		}


		/// <summary>
		/// Provides safe accsess to message
		/// </summary>
		public IMessage Message
		{
			get
			{
				if (targetMessageState == false)
					throw new InvalidOperationException("Message has been deleted manually");

				using (threadLocker.Lock(this))
				{
					if (Channel.HasMessage(PossibleMessageId) == false) CheckAsync().Wait();
					return Channel.GetMessage(PossibleMessageId);
				}
			}
		}


		public async Task StartupMessageAsync()
		{
			if (IsExist) messageWorker(Channel.GetMessage(PossibleMessageId));
			else await RestoreAsync();
		}

		/// <summary>
		/// Id of message that can exist but can dotn't exist
		/// </summary>
		public ulong PossibleMessageId
		{
			get
			{
				using var instance = model.Open();
				return instance.Object.PossibleMessageId;
			}
		}

		/// <summary>
		/// Target channel
		/// </summary>
		public ITextChannelBase Channel
		{
			get
			{
				using var instance = model.Open();
				return instance.Object.Channel;
			}
		}

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
				if (targetMessageState == false) return Task.CompletedTask;
				targetMessageState = false;

				if (Channel.HasMessage(PossibleMessageId)) return Channel.GetMessage(PossibleMessageId).DeleteAsync();
				else return Task.CompletedTask;
			}
		}

		public async Task RestoreAsync()
		{
			using (threadLocker.Lock(this))
			{
				if (targetMessageState == true) return;
				targetMessageState = true;

				if (Channel.HasMessage(PossibleMessageId) == false)
				{
					using var instance = model.Open();
					var message = await SendMessageAsync();
					instance.Object.PossibleMessageId = message.Id;
				}
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
				if (targetMessageState == false) return;

				using var instance = model.Open();

				if (Channel.HasMessage(PossibleMessageId) == false)
					instance.Object.PossibleMessageId = (await SendMessageAsync()).Id;
				else
				{
					var message = Channel.GetMessage(instance.Object.PossibleMessageId);
					await message.ModifyAsync(messageCreator(), true);
					messageWorker(message);
				}
			}
		}

		private async Task<IMessage> SendMessageAsync()
		{
			var send = messageCreator();
			var msg = await Channel.SendMessageAsync(send);
			messageWorker(msg);
			return msg;
		}

		private void OnMessageDeleted(IClient _, IMessage message)
		{
			if (PossibleMessageId != message.Id) return;
			if (targetMessageState == false) return;

			using (threadLocker.Lock(this))
			{
				CheckAsync().Wait();
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (targetMessageState == true)
				throw new InvalidOperationException("Delete message manually before dispose alive holder");
			if (active) Channel.MessageDeleted -= OnMessageDeleted;
			GC.SuppressFinalize(this);
		}

		private async Task CheckAsync()
		{
			if (Channel.HasMessage(PossibleMessageId) == false)
			{
				using var instance = model.Open();
				var message = await SendMessageAsync();
				instance.Object.PossibleMessageId = message.Id;
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
			public Model(ulong possibleMessageId, ITextChannelBase channel)
			{
				PossibleMessageId = possibleMessageId;
				Channel = channel;
			}

			/// <summary>
			/// Creates DidiFrame.Utils.MessageAliveHolder.Model using channel
			/// </summary>
			/// <param name="channel">Channel where need to create message</param>
			public Model(ITextChannelBase channel)
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
			public ITextChannelBase Channel { get; }
		}
	}
}
