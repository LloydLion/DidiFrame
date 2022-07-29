using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// Cache of message for dsharp server
	/// </summary>
	public interface IChannelMessagesCache
	{
		/// <summary>
		/// Adds message to cache
		/// </summary>
		/// <param name="msg">Message to add</param>
		public void AddMessage(DiscordMessage msg);

		/// <summary>
		/// Deletes message from cache
		/// </summary>
		/// <param name="msgId">Id of message</param>
		/// <param name="textChannel">Channel where message was deleted</param>
		public void DeleteMessage(ulong msgId, DiscordChannel textChannel);

		/// <summary>
		/// Gets messages list for channel
		/// </summary>
		/// <param name="textChannel">Channel where need to get messages</param>
		/// <param name="quantity">Quantity of messages</param>
		/// <returns>List of message sorted in chronological order</returns>
		public IReadOnlyList<DiscordMessage> GetMessages(DiscordChannel textChannel, int quantity);

		/// <summary>
		/// Checks message existence in channel
		/// </summary>
		/// <param name="id">Id to check</param>
		/// <param name="textChannel">Channel where need to check existence</param>
		/// <returns>If message exist in channel</returns>
		public bool HasMessage(ulong id, DiscordChannel textChannel);

		/// <summary>
		/// Gets message from cache or null of it doesn't exist
		/// </summary>
		/// <param name="id">Id of message</param>
		/// <param name="textChannel">Channel where need to get message</param>
		/// <returns>Ready message or null</returns>
		public DiscordMessage? GetNullableMessage(ulong id, DiscordChannel textChannel);

		/// <summary>
		/// Gets message from cache 
		/// </summary>
		/// <param name="id">Id of message</param>
		/// <param name="textChannel">Channel where need to get message</param>
		/// <returns>Ready message</returns>
		public DiscordMessage GetMessage(ulong id, DiscordChannel textChannel);

		/// <summary>
		/// Initializes channel to be stored in cache
		/// </summary>
		/// <param name="textChannel">Channel to init</param>
		/// <returns>Operation wait task</returns>
		public Task InitChannelAsync(DiscordChannel textChannel);

		/// <summary>
		/// Deletes channel and all messages from cache
		/// </summary>
		/// <param name="textChannel">Channel to delete</param>
		public void DeleteChannelCache(DiscordChannel textChannel);

		/// <summary>
		/// Provides lock object for channel
		/// </summary>
		/// <param name="textChannel">Channel to get lock object</param>
		/// <returns>Lock object</returns>
		public object Lock(DiscordChannel textChannel);
	}
}
