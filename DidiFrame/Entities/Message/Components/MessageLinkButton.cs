namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Represents a discord link button component
	/// </summary>
	/// <param name="Text">Text that will be displayed in button</param>
	/// <param name="Url">Url of button</param>
	/// <param name="Disabled">If button disabled</param>
	public record MessageLinkButton(string Text, string Url, bool Disabled = false) : IComponent;
}
