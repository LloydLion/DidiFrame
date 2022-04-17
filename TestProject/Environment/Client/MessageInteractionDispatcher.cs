using CGZBot3.Entities.Message.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.Environment.Client
{
	internal class MessageInteractionDispatcher : IInteractionDispatcher
	{
		private readonly List<Handler> handlers = new();
		private readonly Message message;


		public MessageInteractionDispatcher(Message message)
		{
			this.message = message;
		}


		public void Attach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
		{
			handlers.Add(new Handler(callback, id));
		}

		public void Detach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
		{
			handlers.Remove(new Handler(callback, id));
		}

		public Task<ComponentInteractionResult> CallInteraction<TComponent>(string componentId, Member invoker, IComponentState<TComponent>? state = null) where TComponent : IInteractionComponent
		{
			var component = message.SendModel.ComponentsRows?.SelectMany(s => s.Components).SingleOrDefault(s => s is IInteractionComponent ic && ic.Id == componentId)
				?? throw new InvalidOperationException($"No component with {componentId} id message");

			var ctx = new ComponentInteractionContext<TComponent>(invoker, message, (TComponent)component, state);

			return (Task<ComponentInteractionResult>)(handlers.Single(s => s.Id == componentId).Delegate.DynamicInvoke(ctx) ?? throw new NullReferenceException());
		}


		private struct Handler
		{
			public Handler(Delegate @delegate, string id)
			{
				Delegate = @delegate;
				Id = id;
			}


			public Delegate Delegate { get; }

			public string Id { get; }
		}
	}
}