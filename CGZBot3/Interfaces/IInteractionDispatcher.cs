using CGZBot3.Entities.Message.Components;

namespace CGZBot3.Interfaces
{
	public interface IInteractionDispatcher
	{
		public void Attach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent;

		public void Detach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent;
	}
}
