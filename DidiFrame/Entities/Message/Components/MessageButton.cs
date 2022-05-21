namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Represents a discord button component
	/// </summary>
	/// <param name="Id">Unique (per message) id of component</param>
	/// <param name="Text">Text that will be displayed in button</param>
	/// <param name="Style">Style of button</param>
	/// <param name="Disabled">If button disabled</param>
	public record MessageButton(string Id, string Text, ButtonStyle Style, bool Disabled = false) : IInteractionComponent;
}
