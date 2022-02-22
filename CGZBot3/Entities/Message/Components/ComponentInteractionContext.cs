namespace CGZBot3.Entities.Message.Components
{
	public record ComponentInteractionContext<TComponent>(IMember Invoker, IMessage Message, TComponent Component, IComponentState<TComponent>? ComponentState) where TComponent : IComponent;
}
