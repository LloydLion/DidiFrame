namespace DidiFrame.Entities.Message.Components
{
	public record MessageLinkButton(string Text, string Url, bool Disabled = false) : IComponent;
}
