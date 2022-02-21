namespace CGZBot3.Entities.Embed
{
	public class MessageEmbedBuilder
	{
		private string? authorName;
		private string? iconUrl;
		private string? personalUrl;
		private string? imageUrl;
		private DateTime? timestamp;
		private readonly List<EmbedField> fields = new();
		private readonly string title;
		private readonly string description;
		private readonly Color color;


		public MessageEmbedBuilder(string title, string description, Color color)
		{
			this.title = title;
			this.description = description;
			this.color = color;
		}


		public MessageEmbedBuilder AddAuthor(string authorName, string? iconUrl, string? personalUrl)
		{
			this.authorName = authorName;
			this.iconUrl = iconUrl;
			this.personalUrl = personalUrl;
			return this;
		}

		public MessageEmbedBuilder AddImage(string imageUrl)
		{
			this.imageUrl = imageUrl;
			return this;
		}

		public MessageEmbedBuilder AddTimestamp(DateTime timestamp)
		{
			this.timestamp = timestamp;
			return this;
		}

		public MessageEmbedBuilder AddField(EmbedField field)
		{
			fields.Add(field);
			return this;
		}

		public MessageEmbed Build()
		{
			return new MessageEmbed(title, description, color, fields, new EmbedMeta()
			{
				AuthorIconUrl = iconUrl, AuthorName = authorName, AuthorPersonalUrl = personalUrl,
				DisplayImageUrl = imageUrl, Timestamp = timestamp
			});
		}
	}
}
