namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Special model to transmit data about selected options into interaction handlers by select menu component
	/// </summary>
	/// <param name="SelectedValues">Options' values that was selected by uesr</param>
	public record MessageSelectMenuState(IReadOnlyCollection<string> SelectedValues) : IComponentState<MessageSelectMenu>;
}
