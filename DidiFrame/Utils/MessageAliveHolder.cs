using DidiFrame.Data.Model;

namespace DidiFrame.Utils
{
	public delegate MessageSendModel MessageCreator<in TParameter>(TParameter parameter);

	public delegate void MessagePostProcessor<in TParameter>(TParameter parameter, IMessage message);


	/// <summary>
	/// Container for discord message
	/// </summary>
	public class MessageAliveHolder<TParameter>
	{
		private readonly Func<TParameter, MessageAliveHolderModel> selector;
		private readonly MessageCreator<TParameter> creator;
		private readonly MessagePostProcessor<TParameter> postProcessor;
		private readonly AutoResetEvent syncRoot = new(true);
		private bool isFinalized = false;


		public MessageAliveHolder(Func<TParameter, MessageAliveHolderModel> selector, MessageCreator<TParameter> creator, MessagePostProcessor<TParameter> postProcessor)
		{
			this.selector = selector;
			this.creator = creator;
			this.postProcessor = postProcessor;
		}


		private static IMessage GetMessage(MessageAliveHolderModel model) => model.Channel.GetMessage(model.PossibleMessageId);

		public async Task StartupAsync(TParameter parameter)
		{
			syncRoot.WaitOne();
			ThrowIfFinalized();

			var model = selector(parameter);
			var msg = GetMessage(model);

			if (msg.IsExist) postProcessor(parameter, msg);
			else await SendNewMessage(parameter, model);

			syncRoot.Set();
		}

		public ITextChannelBase GetChannel(TParameter parameter) => selector(parameter).Channel;

		public async Task<IMessage> GetMessageAsync(TParameter parameter)
		{
			syncRoot.WaitOne();
			ThrowIfFinalized();

			var msg = await GetMessageAsyncInternal(parameter, selector(parameter));

			syncRoot.Set();

			return msg;
		}

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

		public async Task UpdateAsync(TParameter parameter)
		{
			syncRoot.WaitOne();
			ThrowIfFinalized();

			var model = selector(parameter);
			var message = await GetMessageAsyncInternal(parameter, model);

			var sendModel = creator(parameter);
			await message.ModifyAsync(sendModel, false);

			syncRoot.Set();
		}

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
			postProcessor(parameter, message);
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
	public class MessageAliveHolderModel
	{
		/// <summary>
		/// Creates DidiFrame.Utils.MessageAliveHolder.Model using possible id and channel
		/// </summary>
		/// <param name="possibleMessageId">Possible message id</param>
		/// <param name="channel">Channel where need to create message</param>
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
	}
}
