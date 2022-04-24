namespace DidiFrame.Entities.Message.Components
{
	public record MessageSelectMenuState(IReadOnlyCollection<string> SelectedValues) : IComponentState<MessageSelectMenu>;
}
