namespace DidiFrame.Utils.RoutedEvents
{
	public class RoutedEventTreeNode : IRoutedEventObject
	{
		public delegate Task HandlerExecutor(Func<ValueTask> asyncHandler);


		private readonly HashSet<RoutedEventTreeNode> children = new();
		private RoutedEventTreeNode? parent = null;
		private readonly Dictionary<RoutedEventBase, List<RoutedEventHandler>> handlers = new();
		private readonly HandlerExecutor handlerExecutor;
		private readonly IRoutedEventObject owner;


		public RoutedEventTreeNode(HandlerExecutor handlerExecutor, IRoutedEventObject owner)
		{
			this.handlerExecutor = handlerExecutor;
			this.owner = owner;
		}


		public void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
		{
			GetHandlers(routedEvent).Add(RoutedEventHandler.Cast(handler));
		}

		public void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
		{
			GetHandlers(routedEvent).Remove(RoutedEventHandler.Cast(handler));
		}

		public void AttachParent(RoutedEventTreeNode? node)
		{
			parent?.children.Remove(this);
			parent = node;
			parent?.children.Add(this);
		}

		public Task Invoke<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, TEventArgs args) where TEventArgs : notnull, EventArgs
		{
			return InvokeInternal(routedEvent, args, owner);
		}

		private async Task InvokeInternal<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, TEventArgs args, IRoutedEventObject originalSource) where TEventArgs : notnull, EventArgs
		{
			var eventHandlers = GetHandlers(routedEvent);

			var tasks = new List<Task>();
			foreach (var handler in eventHandlers.Select(s => s.As<TEventArgs>()))
			{
				var sender = new RoutedEventSender(originalSource, owner, () => { owner.RemoveListener(routedEvent, handler); });
				tasks.Add(handlerExecutor.Invoke(() => handler.Invoke(sender, args)));
			}

			switch (routedEvent.Propagation)
			{
				case RoutedEvent.PropagationDirection.None:
					break;

				case RoutedEvent.PropagationDirection.Bubbling:
					parent?.InvokeInternal(routedEvent, args, owner);
					break;

				case RoutedEvent.PropagationDirection.Tunneling:
					await Task.WhenAll(children.Select(child => child.InvokeInternal(routedEvent, args, owner)));
					break;

				default:
					throw new NotSupportedException($"RoutedEventTreeNode doesn't support {routedEvent.Propagation} event propagation type");
			}

			await Task.WhenAll(tasks);
		}

		private List<RoutedEventHandler> GetHandlers(RoutedEventBase routedEvent)
		{
			if (handlers.TryGetValue(routedEvent, out var result))
				return result;
			else
			{
				var list = new List<RoutedEventHandler>();
				handlers.Add(routedEvent, list);
				return list;
			}
		}
	}
}
