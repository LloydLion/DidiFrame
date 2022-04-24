namespace DidiFrame.Entities.Message.Components
{
	public delegate Task<ComponentInteractionResult> AsyncInteractionCallback<TComponent>(ComponentInteractionContext<TComponent> context) where TComponent : IComponent;
}
