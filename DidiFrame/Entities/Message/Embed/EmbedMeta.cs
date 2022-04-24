namespace DidiFrame.Entities.Message.Embed
{
	public record EmbedMeta
	{
		public string? AuthorName { get; init; }

		public string? AuthorIconUrl { get; init; }

		public string? AuthorPersonalUrl { get; init; }

		public string? DisplayImageUrl { get; init; }

		public DateTime? Timestamp { get; init; }
}
}