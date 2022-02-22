namespace CGZBot3.Entities.Message.Components
{
	public record MessageSelectMenuState(IReadOnlyCollection<MessageSelectMenuOption> SelectedOptions) : IComponentState<MessageSelectMenu>;
}
