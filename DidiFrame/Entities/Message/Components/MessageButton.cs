namespace DidiFrame.Entities.Message.Components
{
	public record MessageButton(string Id, string Text, ButtonStyle Style, bool Disabled = false) : IInteractionComponent
	{ }
}
