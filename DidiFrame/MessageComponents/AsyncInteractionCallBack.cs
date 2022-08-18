namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Event hadler for interaction with discord component
	/// </summary>
	/// <typeparam name="TComponent">Target compoent type</typeparam>
	/// <param name="context">Interaction context that contains all information about interaction</param>
	/// <returns>Task with special model that contains responce info and will be sent to user</returns>
	public delegate Task<ComponentInteractionResult> AsyncInteractionCallback<TComponent>(ComponentInteractionContext<TComponent> context) where TComponent : IInteractionComponent;
}
