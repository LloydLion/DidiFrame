namespace DidiFrame.Entities.Message.Embed
{
	/// <summary>
	/// Represents a message's embed
	/// </summary>
	/// <param name="Title">Title of embed</param>
	/// <param name="Description">Description of embed</param>
	/// <param name="Color">Color of embed</param>
	/// <param name="Fields">Collection of fields</param>
	/// <param name="Metadata">Contains all other data for embed</param>
	public record MessageEmbed(string Title, string Description, Color Color, IReadOnlyCollection<EmbedField> Fields, EmbedMeta Metadata);
}
