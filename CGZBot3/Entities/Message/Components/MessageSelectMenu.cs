namespace CGZBot3.Entities.Message.Components
{
	public record MessageSelectMenu(IReadOnlyList<MessageSelectMenuOption> Options, AsyncInteractionCallback<MessageSelectMenu> Callback, string PlaceHolder, int MinOptions = 1, int MaxOptions = 1, bool Disabled = false) : IComponent
	{
		public Task<ComponentInteractionResult> HandleAsync(ComponentInteractionContext<MessageSelectMenu> context) => Callback(context);
	}
}
