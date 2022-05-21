namespace DidiFrame.Entities.Message
{
	/// <summary>
	/// Model to send messages, it is all "extenal" state of discord message
	/// </summary>
	/// <param name="Content">Content of message</param>
	public record MessageSendModel(string? Content = null)
	{
		/// <summary>
		/// List (ordered) of embeds in message
		/// </summary>
		public IReadOnlyList<MessageEmbed>? MessageEmbeds { get; init; }

		/// <summary>
		/// Collection of attached files in message
		/// </summary>
		public IReadOnlyCollection<MessageFile>? Files { get; init; }

		/// <summary>
		/// Collection of MessageComponentsRow objects that contains message's components
		/// </summary>
		public IReadOnlyCollection<MessageComponentsRow>? ComponentsRows { get; init; }
	}
}