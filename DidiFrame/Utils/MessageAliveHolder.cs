using DidiFrame.Data.Model;

namespace DidiFrame.Utils
{
	/// <summary>
	/// Delegate that creates messages using some parameter
	/// </summary>
	/// <typeparam name="TParameter">Type of parameter</typeparam>
	/// <param name="parameter">Parameter itself</param>
	/// <returns>Send model for message</returns>
	public delegate MessageSendModel MessageCreator<in TParameter>(TParameter parameter);

	/// <summary>
	/// Delegate that processes some messages using some parameter
	/// </summary>
	/// <typeparam name="TParameter">Type of parameter</typeparam>
	/// <param name="parameter">Parameter itself</param>
	/// <param name="message">Message to process</param>
	/// <param name="isModified">If message has been modified</param>
	public delegate void MessagePostProcessor<in TParameter>(TParameter parameter, IMessage message, bool isModified);


	/// <summary>
	/// Container for discord message
	/// </summary>
	/// <typeparam name="TParameter">Type of parameter</typeparam>
	public class MessageAliveHolder<TParameter>
	{
		private readonly Func<TParameter, MessageAliveHolderModel> selector;
		private readonly MessageCreator<TParameter> creator;
		private readonly MessagePostProcessor<TParameter> postProcessor;
		private readonly AutoResetEvent syncRoot = new(true);
		private bool isFinalized = false;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.MessageAliveHolder`1
		/// </summary>
		/// <param name="selector">Base model selector from parameter</param>
		/// <param name="creator">Messages creater to create new or modify old messages</param>
		/// <param name="postProcessor">Post processor for messages</param>
		public MessageAliveHolder(Func<TParameter, MessageAliveHolderModel> selector, MessageCreator<TParameter> creator, MessagePostProcessor<TParameter> postProcessor)
		{
			this.selector = selector;
			this.creator = creator;
			this.postProcessor = postProcessor;
		}


		private static IMessage GetMessage(MessageAliveHolderModel model) => model.Channel.GetMessage(model.PossibleMessageId);

		/// <summary>
		/// Init method, checks message existance, if need restores it
		/// </summary>
		/// <param name="parameter">Parameter</param>
		/// <returns>Operation wait task</returns>
		/// <exception cref="InvalidOperationException">If holder is finalized</exception>
		public async Task StartupAsync(TParameter parameter)
		{
			syncRoot.WaitOne();
			ThrowIfFinalized();

			var model = selector(parameter);
			var msg = GetMessage(model);

			if (msg.IsExist) postProcessor(parameter, msg, isModified: false);
			else await SendNewMessage(parameter, model);

			syncRoot.Set();
		}

		/// <summary>
		/// Gets channel where message was sent
		/// </summary>
		/// <param name="parameter">Parameter</param>
		/// <returns>Text channel that contains message (or not exist)</returns>
		public ITextChannelBase GetChannel(TParameter parameter) => selector(parameter).Channel;

		/// <summary>
		/// Gets message, can restore it if need
		/// </summary>
		/// <param name="parameter">Parameter</param>
		/// <returns>Wait task with message</returns>
		/// <exception cref="InvalidOperationException">If holder is finalized</exception>
		public async Task<IMessage> GetMessageAsync(TParameter parameter)
		{
			syncRoot.WaitOne();
			ThrowIfFinalized();

			var msg = await GetMessageAsyncInternal(parameter, selector(parameter));

			syncRoot.Set();

			return msg;
		}

		/// <summary>
		/// Finalizes holder, deletes message
		/// </summary>
		/// <param name="parameter">Parameter</param>
		/// <returns>Operation wait task</returns>
		/// <exception cref="InvalidOperationException">If holder is finalized</exception>
		public async Task FinalizeAsync(TParameter parameter)
		{
			syncRoot.WaitOne();
			ThrowIfFinalized();

			isFinalized = true;

			var model = selector(parameter);
			var msg = GetMessage(model);

			if (msg.IsExist) await msg.DeleteAsync();

			syncRoot.Set();
		}

		/// <summary>
		/// Updates message async using new model form message creator (calls post processor)
		/// </summary>
		/// <param name="parameter">Parameter</param>
		/// <returns>Wait task</returns>
		/// <exception cref="InvalidOperationException">If holder is finalized</exception>
		public async Task UpdateAsync(TParameter parameter)
		{
			syncRoot.WaitOne();
			ThrowIfFinalized();

			var model = selector(parameter);
			var message = await GetMessageAsyncInternal(parameter, model);

			var sendModel = creator(parameter);
			await message.ModifyAsync(sendModel, resetDispatcher: false);
			postProcessor(parameter, message, isModified: true);

			syncRoot.Set();
		}

		/// <summary>
		/// Special method that should call in message deleted event handler, it automaticlly restores message if it will be deleted
		/// </summary>
		/// <param name="parameter">Parameter</param>
		/// <param name="textChannel">Text channel where some message deleted</param>
		/// <param name="messageId">Id of deleted message</param>
		/// <returns>Operation wait task</returns>
		/// <exception cref="InvalidOperationException">If holder is finalized</exception>
		public async Task OnMessageDeleted(TParameter parameter, ITextChannelBase textChannel, ulong messageId)
		{
			syncRoot.WaitOne();
			ThrowIfFinalized();

			var model = selector(parameter);
			if (model.Channel.Equals(textChannel) && model.PossibleMessageId == messageId)
				await GetMessageAsyncInternal(parameter, model);

			syncRoot.Set();
		}

		private async Task<IMessage> SendNewMessage(TParameter parameter, MessageAliveHolderModel model)
		{
			var channel = model.Channel;
			var sendModel = creator(parameter);
			var message = await channel.SendMessageAsync(sendModel);
			postProcessor(parameter, message, isModified: false);
			model.PossibleMessageId = message.Id;
			return message;
		}

		private async Task<IMessage> GetMessageAsyncInternal(TParameter parameter, MessageAliveHolderModel model)
		{
			var msg = GetMessage(model);

			if (!msg.IsExist) msg = await SendNewMessage(parameter, model);

			return msg;
		}

		private void ThrowIfFinalized()
		{
			if (isFinalized)
			{
				syncRoot.Set();
				throw new InvalidOperationException("MessageAliveHolder finalized and you cannot do anything with this");
			}
		}
	}

	/// <summary>
	/// Serialization model of DidiFrame.Utils.MessageAliveHolder
	/// </summary>
	public class MessageAliveHolderModel : IDataModel
	{
		/// <summary>
		/// Creates DidiFrame.Utils.MessageAliveHolder.Model using possible id and channel
		/// </summary>
		/// <param name="possibleMessageId">Possible message id</param>
		/// <param name="channel">Channel where need to create message</param>
		[SerializationConstructor]
		public MessageAliveHolderModel(ulong possibleMessageId, ITextChannelBase channel)
		{
			PossibleMessageId = possibleMessageId;
			Channel = channel;
		}

		/// <summary>
		/// Creates DidiFrame.Utils.MessageAliveHolder.Model using channel
		/// </summary>
		/// <param name="channel">Channel where need to create message</param>
		public MessageAliveHolderModel(ITextChannelBase channel)
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

		/// <inheritdoc/>
		public Guid Id { get; } = Guid.NewGuid();


		/// <inheritdoc/>
		public bool Equals(IDataModel? other) => false;
	}
}
