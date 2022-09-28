using DidiFrame.Entities.Message;
using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test IMessage implementation
	/// </summary>
	public class Message : IMessage
	{
		private Lazy<MessageInteractionDispatcher> mid;


		internal Message(MessageSendModel sendModel, TextChannelBase baseTextChannel, Member author)
		{
			SendModel = sendModel;
			BaseTextChannel = baseTextChannel;
			Id = baseTextChannel.BaseServer.BaseClient.GenerateNextId();
			Author = author;
			mid = new Lazy<MessageInteractionDispatcher>(() => new MessageInteractionDispatcher(this));
		}


		/// <inheritdoc/>
		public MessageSendModel SendModel { get; private set; }

		/// <inheritdoc/>
		public ulong Id { get; }

		/// <inheritdoc/>
		public ITextChannelBase TextChannel => BaseTextChannel;

		/// <summary>
		/// Base text channel that contains this message
		/// </summary>
		public TextChannelBase BaseTextChannel { get; }

		/// <inheritdoc/>
		public IMember Author { get; }

		/// <inheritdoc/>
		public bool IsExist => BaseTextChannel.HasMessage(Id);


		/// <inheritdoc/>
		public Task DeleteAsync()
		{
			BaseTextChannel.DeleteMessage(this);
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public bool Equals(IMessage? other) => other is Message message && message.Id == Id;

		/// <inheritdoc/>
		public IInteractionDispatcher GetInteractionDispatcher() => GetBaseInteractionDispatcher();

		/// <summary>
		/// Gets base interaction dispatcher for this message
		/// </summary>
		/// <returns>New interaction dispatcher or cached old instance</returns>
		public MessageInteractionDispatcher GetBaseInteractionDispatcher() => mid.Value;

		/// <inheritdoc/>
		public Task ModifyAsync(MessageSendModel sendModel, bool resetDispatcher)
		{
			if (resetDispatcher) ResetInteractionDispatcher();
			BaseTextChannel.EditMessage(this, sendModel);
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public void ResetInteractionDispatcher()
		{
			if (mid.IsValueCreated) mid = new Lazy<MessageInteractionDispatcher>(() => new MessageInteractionDispatcher(this));
		}

		internal void ModifyInternal(MessageSendModel sendModel)
		{
			SendModel = sendModel;
		}
	}
}
