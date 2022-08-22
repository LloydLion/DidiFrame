namespace DidiFrame.Clients
{
	/// <summary>
	/// Represents some text-like channel (thread, voice, text)
	/// </summary>
	public interface ITextChannelBase : IChannel
	{
		/// <summary>
		/// Send message to channel async
		/// </summary>
		/// <param name="messageSendModel">Send model of new message</param>
		/// <returns>Task with new message</returns>
		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel);

		/// <summary>
		/// Taskes all or given count of messages. Sorting: from new to old
		/// </summary>
		/// <param name="count">Count of messages or -1 if need all possible messages</param>
		/// <returns>List of messages</returns>
		public IReadOnlyList<IMessage> GetMessages(int count = -1);

		/// <summary>
		/// Gets message by id
		/// </summary>
		/// <param name="id">Id of message</param>
		/// <returns>Message that has given id</returns>
		public IMessage GetMessage(ulong id);

		/// <summary>
		/// Checks if channel contains message with same id
		/// </summary>
		/// <param name="id">Id for checking</param>
		/// <returns>If channel contains</returns>
		public bool HasMessage(ulong id);


		/// <summary>
		/// Event that fired when a message has sent
		/// </summary>
		public event MessageSentEventHandler? MessageSent;

		/// <summary>
		/// Event that fired when a message has deleted
		/// </summary>
		public event MessageDeletedEventHandler? MessageDeleted;
	}
}