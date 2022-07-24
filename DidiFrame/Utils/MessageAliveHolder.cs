using DidiFrame.Data.Model;
using System.Data.Common;
using System.Reflection.Metadata;

namespace DidiFrame.Utils
{
	/// <summary>
	/// Container for discord message
	/// </summary>
	public class MessageAliveHolder : IDisposable
	{
		private readonly Func<object?, MessageSendModel> messageCreator;
		private readonly Action<object?, IMessage> messageWorker;
		private readonly static ThreadLocker<MessageAliveHolder> threadLocker = new();
		private readonly Func<object?, ObjectHolder<Model>> modelGetter;
		private readonly bool active;
		private bool targetMessageState;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.MessageAliveHolder using base serialization model
		/// </summary>
		/// <param name="model">Base model for DidiFrame.Utils.MessageAliveHolder object</param>
		/// <param name="active">If need display message always unless only by request</param>
		/// <param name="messageCreator">Creator of message that can create msg's send model by request</param>
		/// <param name="messageWorker">Post propessor for created message</param>
		public MessageAliveHolder(Model initialModel, Func<object?, ObjectHolder<Model>> modelGetter, bool active, Func<object?, MessageSendModel> messageCreator, Action<object?, IMessage> messageWorker)
		{
			this.messageCreator = messageCreator;
			this.messageWorker = messageWorker;
			this.modelGetter = modelGetter;
			this.active = active;

			Channel = initialModel.Channel;
			if (active) Channel.MessageDeleted += OnMessageDeleted;
		}


		/// <summary>
		/// Provides safe accsess to message
		/// </summary>
		public IMessage GetMessage(object? parameter)
		{
			if (targetMessageState == false)
				throw new InvalidOperationException("Message has been deleted manually");

			using (threadLocker.Lock(this))
			{
				if (Channel.HasMessage(GetPossibleMessageId(parameter)) == false) CheckAsync(parameter).Wait();
				return Channel.GetMessage(GetPossibleMessageId(parameter));
			}
		}


		public async Task StartupMessageAsync(object? parameter)
		{
			if (GetIsExist(parameter)) messageWorker(parameter, Channel.GetMessage(GetPossibleMessageId(parameter)));
			else await RestoreAsync(parameter);
		}

		/// <summary>
		/// Id of message that can exist but can dotn't exist
		/// </summary>
		public ulong GetPossibleMessageId(object? parameter)
		{
			using var instance = modelGetter(parameter);
			return instance.Object.PossibleMessageId;
		}

		/// <summary>
		/// Target channel
		/// </summary>
		public ITextChannelBase Channel { get; }

		/// <summary>
		/// If need display message always unless only by request
		/// </summary>
		public bool ActiveMode => active;

		/// <summary>
		/// If message still exist
		/// </summary>
		public bool GetIsExist(object? parameter) => Channel.HasMessage(GetPossibleMessageId(parameter));


		/// <summary>
		/// Deletes message async
		/// </summary>
		/// <returns>Wait task</returns>
		public Task DeleteAsync(object? parameter)
		{
			using (threadLocker.Lock(this))
			{
				targetMessageState = false;
				if (GetIsExist(parameter)) return Channel.GetMessage(GetPossibleMessageId(parameter)).DeleteAsync();
				else return Task.CompletedTask;
			}
		}

		public async Task RestoreAsync(object? parameter)
		{
			using (threadLocker.Lock(this))
			{
				targetMessageState = true;

				if (GetIsExist(parameter) == false)
				{
					var message = await SendMessageAsync(parameter);
					using var instance = modelGetter(parameter);
					instance.Object.PossibleMessageId = message.Id;
				}
			}
		}

		/// <summary>
		/// Modifies message using new send model from messages creator
		/// </summary>
		/// <returns>Wait task</returns>
		public async Task Update(object? parameter)
		{
			using (threadLocker.Lock(this))
			{
				if (targetMessageState == false) return;

				using var instance = modelGetter(parameter);

				if (Channel.HasMessage(GetPossibleMessageId(parameter)) == false)
					instance.Object.PossibleMessageId = (await SendMessageAsync(parameter)).Id;
				else
				{
					var message = Channel.GetMessage(instance.Object.PossibleMessageId);
					await message.ModifyAsync(messageCreator(parameter), true);
					messageWorker(parameter, message);
				}
			}
		}

		private async Task<IMessage> SendMessageAsync(object? parameter)
		{
			var send = messageCreator(parameter);
			var msg = await Channel.SendMessageAsync(send);
			messageWorker(parameter, msg);
			return msg;
		}

		private void OnMessageDeleted(IClient _, ITextChannelBase textChannel, ulong msgId)
		{
			if (GetPossibleMessageId(null) != msgId) return;
			if (targetMessageState == false) return;

			RestoreAsync(null).Wait();
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (targetMessageState == true)
				throw new InvalidOperationException("Delete message manually before dispose alive holder");
			if (active) Channel.MessageDeleted -= OnMessageDeleted;
			GC.SuppressFinalize(this);
		}

		private async Task CheckAsync(object? parameter)
		{
			if (Channel.HasMessage(GetPossibleMessageId(parameter)) == false)
			{
				using var instance = modelGetter(parameter);
				var message = await SendMessageAsync(parameter);
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
