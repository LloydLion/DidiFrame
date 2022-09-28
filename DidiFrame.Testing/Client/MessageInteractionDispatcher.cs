using DidiFrame.Entities.Message.Components;
using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test IInteractionDispatcher implementation
	/// </summary>
	public class MessageInteractionDispatcher : IInteractionDispatcher
	{
		private readonly List<Handler> handlers = new();
		private readonly Message message;


		/// <summary>
		/// Creates new instance of DidiFrame.Testing.Client.MessageInteractionDispatcher
		/// </summary>
		/// <param name="message">Target message for dispatcher</param>
		public MessageInteractionDispatcher(Message message)
		{
			this.message = message;
		}


		/// <inheritdoc/>
		public void Attach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
		{
			handlers.Add(new Handler(callback, id));
		}

		/// <inheritdoc/>
		public void Detach<TComponent>(string id, AsyncInteractionCallback<TComponent> callback) where TComponent : IInteractionComponent
		{
			handlers.Remove(new Handler(callback, id));
		}

		/// <summary>
		/// Calls demo interaction
		/// </summary>
		/// <typeparam name="TComponent">Type of target component</typeparam>
		/// <param name="componentId">Target component id</param>
		/// <param name="invoker">Invoker that calls interaction</param>
		/// <param name="state">State for component</param>
		/// <returns>Task with interaction result</returns>
		/// <exception cref="InvalidOperationException">If component with given doesn't exist</exception>
		public Task<ComponentInteractionResult> CallInteractionAsync<TComponent>(string componentId, Member invoker, IComponentState<TComponent>? state = null) where TComponent : IInteractionComponent
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