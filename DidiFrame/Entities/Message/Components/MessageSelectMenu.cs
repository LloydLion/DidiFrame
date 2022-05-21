namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Represents a discord select menu component. Must be only one in components row (discord requirement). Has component state - DidiFrame.Entities.Message.Components.MessageSelectMenuState
	/// </summary>
	/// <param name="Id">Unique (per message) id of component</param>
	/// <param name="Options">Menu options</param>
	/// <param name="PlaceHolder">Place holder of menu</param>
	/// <param name="MinOptions">Min options count that can be selected</param>
	/// <param name="MaxOptions">Max options count that can be selected</param>
	/// <param name="Disabled">If menu disabled</param>
	public record MessageSelectMenu(string Id, IReadOnlyList<MessageSelectMenuOption> Options, string PlaceHolder, int MinOptions = 1, int MaxOptions = 1, bool Disabled = false) : IInteractionComponent;
}
