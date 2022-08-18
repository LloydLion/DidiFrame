namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Special model to transmit addititional data into interaction handlers by component,
	/// need to be up-casted to component-specific type to be useful
	/// </summary>
	/// <typeparam name="TComponent">Target compoent type</typeparam>
	public interface IComponentState<TComponent> where TComponent : IInteractionComponent
	{

	}
}