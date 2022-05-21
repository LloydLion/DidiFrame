namespace DidiFrame.Entities
{
	/// <summary>
	/// Model to create discord channel
	/// </summary>
	/// <param name="Name">Name of new channel</param>
	/// <param name="ChannelType">Type of new channel</param>
	public record ChannelCreationModel(string Name, ChannelType ChannelType);
}
