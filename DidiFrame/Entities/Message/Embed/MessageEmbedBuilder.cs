namespace DidiFrame.Entities.Message.Embed
{
	/// <summary>
	/// Builder for more convenient work with DidiFrame.Entities.Message.Embed.MessageEmbed
	/// </summary>
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


		/// <summary>
		/// Creates instance of DidiFrame.Entities.Message.Embed.MessageEmbedBuilder
		/// </summary>
		/// <param name="title">Title of future embed</param>
		/// <param name="description">Description of future embed</param>
		/// <param name="color">Color of embed</param>
		public MessageEmbedBuilder(string title, string description, Color color)
		{
			this.title = title;
			this.description = description;
			this.color = color;
		}


		/// <summary>
		/// Adds author info into future embed
		/// </summary>
		/// <param name="authorName">Author's name</param>
		/// <param name="iconUrl">Author's icon url</param>
		/// <param name="personalUrl">Author's page url</param>
		/// <returns>Builder to be chained</returns>
		public MessageEmbedBuilder AddAuthor(string authorName, string? iconUrl, string? personalUrl)
		{
			this.authorName = authorName;
			this.iconUrl = iconUrl;
			this.personalUrl = personalUrl;
			return this;
		}

		/// <summary>
		/// Adds image into future embed
		/// </summary>
		/// <param name="imageUrl">Url to image</param>
		/// <returns>Builder to be chained</returns>
		public MessageEmbedBuilder AddImage(string imageUrl)
		{
			this.imageUrl = imageUrl;
			return this;
		}

		/// <summary>
		/// Adds timestamp into future embed
		/// </summary>
		/// <param name="timestamp">Timestamp as System.DateTime</param>
		/// <returns>Builder to be chained</returns>
		public MessageEmbedBuilder AddTimestamp(DateTime timestamp)
		{
			this.timestamp = timestamp;
			return this;
		}

		/// <summary>
		/// Adds field into future embed
		/// </summary>
		/// <param name="field">Field model</param>
		/// <returns>Builder to be chained</returns>
		public MessageEmbedBuilder AddField(EmbedField field)
		{
			fields.Add(field);
			return this;
		}

		/// <summary>
		/// Builds final embed
		/// </summary>
		/// <returns>Builden embed</returns>
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
