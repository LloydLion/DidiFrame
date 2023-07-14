using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Entities.Message.Components;

namespace DidiFrame.Clients.DSharp.Interactions
{
	public class MessageInteractionDispatcher : IInteractionDispatcher
	{
		private readonly ulong messageId;
		private readonly MessageRepository repository;


		public MessageInteractionDispatcher(ulong messageId, MessageRepository repository)
		{
			this.messageId = messageId;
			this.repository = repository;
		}


		public bool HasSubscribers { get; }


		public void Attach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
		{
			throw new NotImplementedException();
		}

		public void Detach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
		{
			throw new NotImplementedException();
		}
	}
}
