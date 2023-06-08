namespace DidiFrame.Clients
{
	/// <summary>
	/// Middle layer between you and discord message's components
	/// </summary>
	public interface IInteractionDispatcher
	{
		/// <summary>
		/// Attaches handler to message's component
		/// </summary>
		/// <typeparam name="TComponent">Type of component</typeparam>
		/// <param name="id">Id of component</param>
		/// <param name="callback">Event handler</param>
		public void Attach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent;

		/// <summary>
		/// Detaches handler to message's component
		/// </summary>
		/// <typeparam name="TComponent">Type of component</typeparam>
		/// <param name="id">Id of component</param>
		/// <param name="callback">Event handler</param>
		public void Detach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent;
	}
}