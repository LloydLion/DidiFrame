namespace DidiFrame.Entities.Message.Embed
{
	/// <summary>
	/// Represents a field of embed
	/// </summary>
	/// <param name="Name">Title of field</param>
	/// <param name="Value">Content of field</param>
	public record EmbedField(string Name, string Value);
}