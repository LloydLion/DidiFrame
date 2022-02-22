namespace CGZBot3.Entities.Message.Components
{
	public record MessageButton(string Text, ButtonStyle Style, AsyncInteractionCallback<MessageButton> Callback, bool Disabled = false) : IComponent
	{
		public Task<ComponentInteractionResult> HandleAsync(ComponentInteractionContext<MessageButton> ctx) => Callback(ctx);
	}
}
