namespace CGZBot3.Entities.Message.Components
{
	public record MessageSelectMenu(IReadOnlyList<MessageSelectMenuOption> Options, string PlaceHolder, int MinOptions = 1, int MaxOptions = 1, bool Disabled = false) : IComponent
	{ }
}
