namespace DidiFrame.Entities.Message.Embed
{
	/// <summary>
	/// Represents all additional data for discord embed
	/// </summary>
	public record EmbedMeta
	{
		/// <summary>
		/// Name of author
		/// </summary>
		public string? AuthorName { get; init; }

		/// <summary>
		/// Url to author's profile icon. It will be showed next to name
		/// </summary>
		public string? AuthorIconUrl { get; init; }

		/// <summary>
		/// Url to author's personal page. If not null name and icon of author will be clickable
		/// </summary>
		public string? AuthorPersonalUrl { get; init; }

		/// <summary>
		/// Url to image for display in embed
		/// </summary>
		public string? DisplayImageUrl { get; init; }

		/// <summary>
		/// Some timestamp that will be showed in footer of embed
		/// </summary>
		public DateTime? Timestamp { get; init; }
}
}