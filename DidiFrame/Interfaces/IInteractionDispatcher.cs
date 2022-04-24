using DidiFrame.Entities.Message.Components;

namespace DidiFrame.Interfaces
{
	public interface IInteractionDispatcher
	{
		public void Attach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent;

		public void Detach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent;
	}
}
