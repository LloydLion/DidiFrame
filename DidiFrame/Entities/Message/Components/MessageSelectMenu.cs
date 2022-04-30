namespace DidiFrame.Entities.Message.Components
{
	public record MessageSelectMenu(string Id, IReadOnlyList<MessageSelectMenuOption> Options, string PlaceHolder, int MinOptions = 1, int MaxOptions = 1, bool Disabled = false) : IInteractionComponent
	{ }
}
