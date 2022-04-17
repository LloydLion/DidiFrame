using CGZBot3.Entities.Message.Components;
using System;
using System.Collections.Generic;
using System.Linq;

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


		public void Attach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IComponent
		{
			handlers.Add(new Handler(callback, id));
		}

		public void Detach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IComponent
		{
			handlers.Remove(new Handler(callback, id));
		}

		public ComponentInteractionResult CallInteraction<TComponent>(ComponentInteractionContext<TComponent> ctx) where TComponent : IComponent
		{
			if (ctx.Message != message) throw new ArgumentException("Invalid message in context", nameof(ctx));
			var id = (string)(ctx.Component.GetType()?.GetProperty("Id")?.GetValue(ctx.Component) ?? throw new ArgumentException("Enable to call interaction for component in context", nameof(ctx)));
			return (ComponentInteractionResult)(handlers.Single(s => s.Id == id).Delegate.DynamicInvoke(ctx) ?? throw new NullReferenceException());
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