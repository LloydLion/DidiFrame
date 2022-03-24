using CGZBot3.Entities.Message.Components;

namespace CGZBot3.Interfaces
{
	public interface IInteractionObserver
	{
		public void Observe<TComponent>(IMessage message, IComponent component, AsyncInteractionCallback<TComponent> callback) where TComponent : IComponent;
	}
}
