namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Context that contains all information about interaction
	/// </summary>
	/// <typeparam name="TComponent">Target compoent type</typeparam>
	/// <param name="Invoker">Member that has called interaction</param>
	/// <param name="Message">Message that contains interaction component</param>
	/// <param name="Component">Component itself</param>
	/// <param name="ComponentState">State of component, need to be up-casted to component-specific type to be useful</param>
	public record ComponentInteractionContext<TComponent>(IMember Invoker, IMessage Message, TComponent Component, IComponentState<TComponent>? ComponentState) where TComponent : IInteractionComponent;
}
