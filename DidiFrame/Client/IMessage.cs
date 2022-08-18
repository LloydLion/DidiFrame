using DidiFrame.Entities.Message;

namespace DidiFrame.Client
{
	/// <summary>
	/// Represents a discord message
	/// </summary>
	public interface IMessage : IEquatable<IMessage>
	{
		/// <summary>
		/// Send model of the message
		/// </summary>
		public MessageSendModel SendModel { get; }

		/// <summary>
		/// Message's content (from SendModel)
		/// </summary>
		public string? Content { get { return SendModel.Content; } }

		/// <summary>
		/// Id of the message
		/// </summary>
		public ulong Id { get; }

		/// <summary>
		/// Text channel that contains this message
		/// </summary>
		public ITextChannelBase TextChannel { get; }

		/// <summary>
		/// Author of the message
		/// </summary>
		public IMember Author { get; }

		/// <summary>
		/// If message still exists in channel
		/// </summary>
		public bool IsExist { get; }


		/// <summary>
		/// Deletes the message async
		/// </summary>
		/// <returns>Wait task</returns>
		public Task DeleteAsync();

		/// <summary>
		/// Gets interaction dispatcher for this message
		/// </summary>
		/// <returns>New interaction dispatcher or cached old instance</returns>
		public IInteractionDispatcher GetInteractionDispatcher();

		/// <summary>
		/// Modifies message
		/// </summary>
		/// <param name="sendModel">New send model</param>
		/// <param name="resetDispatcher">If need to erase interaction dispatcher and create new</param>
		/// <returns>Wait task</returns>
		public Task ModifyAsync(MessageSendModel sendModel, bool resetDispatcher);

		/// <summary>
		/// Erases interaction dispatcher and creates new
		/// </summary>
		public void ResetInteractionDispatcher();
	}
}