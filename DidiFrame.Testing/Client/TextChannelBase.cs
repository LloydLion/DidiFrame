using DidiFrame.Entities.Message;
using DidiFrame.Clients;
using DidiFrame.Exceptions;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test ITextChannelBase implementation
	/// </summary>
	public class TextChannelBase : Channel, ITextChannelBase
	{
		internal TextChannelBase(string name, ChannelCategory category) : base(name, category) { }


		/// <inheritdoc/>
		public event MessageSentEventHandler? MessageSent;

		/// <inheritdoc/>
		public event MessageDeletedEventHandler? MessageDeleted;


		private readonly List<Message> msgs = new();


		/// <inheritdoc/>
		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			var message = AddMessage((Member)BaseServer.GetMember(BaseServer.Client.SelfAccount), messageSendModel);
			return Task.FromResult((IMessage)message);
		}

		/// <inheritdoc/>
		public IReadOnlyList<IMessage> GetMessages(int count = -1)
		{
			return count == -1 ? GetIfExist(msgs) : GetIfExist(msgs).Take(count).ToArray();
		}

		/// <inheritdoc/>
		public IMessage GetMessage(ulong id)
		{
			return GetIfExist(msgs).Single(s => s.Id == id);
		}

		/// <inheritdoc/>
		public bool HasMessage(ulong id)
		{
			return GetIfExist(msgs).Any(s => s.Id == id);
		}

		/// <summary>
		/// Sends message in this channel
		/// </summary>
		/// <param name="sender">Message sender</param>
		/// <param name="sendModel">Send model of new message</param>
		/// <returns>New message</returns>
		public Message AddMessage(Member sender, MessageSendModel sendModel)
		{
			var message = new Message(sendModel, this, sender);
			GetIfExist(msgs).Add(message);

			try { MessageSent?.Invoke(BaseCategory.BaseServer.BaseClient, message, false); }
			catch (Exception) { /*No logging*/ }
			BaseServer.OnMessageCreated(message, false);

			return message;
		}

		/// <summary>
		/// Edits given message
		/// </summary>
		/// <param name="message">Message to edit</param>
		/// <param name="sendModel">Send model for edit</param>
		/// <exception cref="ObjectDoesNotExistException">If channel is not exist</exception>
		/// <exception cref="ArgumentException">If channel doesn't contain this message</exception>
		public void EditMessage(Message message, MessageSendModel sendModel)
		{
			if (Equals(message.TextChannel, this) == false)
			{
				throw new ArgumentException("Channel doesn't contain this message", nameof(message));
			}

			GetIfExist(false);

			message.ModifyInternal(sendModel);

			try { MessageSent?.Invoke(BaseCategory.BaseServer.BaseClient, message, true); }
			catch (Exception) { /*No logging*/ }
			BaseServer.OnMessageCreated(message, true);
		}

		/// <summary>
		/// Deletes given message
		/// </summary>
		/// <param name="message">Message to delete</param>
		/// <exception cref="ObjectDoesNotExistException">If channel is not exist</exception>
		/// <exception cref="ArgumentException">If channel doesn't contain this message</exception>
		public void DeleteMessage(Message message)
		{
			if (Equals(message.TextChannel, this) == false)
			{
				throw new ArgumentException("Channel doesn't contain this message", nameof(message));
			}

			GetIfExist(msgs).Remove(message);

			try { MessageDeleted?.Invoke(BaseCategory.BaseServer.BaseClient, this, message.Id); }
			catch (Exception) { /*No logging*/ }
			BaseServer.OnMessageDeleted(this, message.Id);
		}
	}
}
