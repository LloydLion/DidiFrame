namespace DidiFrame.Utils.RoutedEvents
{
	public class RoutedEventTreeNode : IRoutedEventObject
	{
		public delegate Task HandlerExecutor(Func<ValueTask> asyncHandler);


		private readonly HashSet<RoutedEventTreeNode> children = new();
		private RoutedEventTreeNode? parent = null;
		private readonly Dictionary<RoutedEventBase, List<RoutedEventHandler>> handlers = new();
		private readonly IRoutedEventObject owner;
		private HandlerExecutor? overrideHandlerExecutor;
		private CompositionRoot? cachedRoot;


		public RoutedEventTreeNode(IRoutedEventObject owner, HandlerExecutor? overrideHandlerExecutor = null)
		{
			this.owner = owner;
			this.overrideHandlerExecutor = overrideHandlerExecutor;
		}


		public bool HasHandlers => handlers.Count != 0;

		private HandlerExecutor DelegatedHandlerExecutor
		{
			get
			{
				return overrideHandlerExecutor ?? parent?.DelegatedHandlerExecutor ?? throw new InvalidOperationException("No handler executor in routed event tree");
			}
		}

		private CompositionRoot Root
		{
			get
			{
				if (parent is null)
				{
					cachedRoot ??= new CompositionRoot();
					return cachedRoot;
				}
				else
				{
					cachedRoot = null;
					return parent.Root;
				}
			}
		}


		public void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
		{
			lock (Root.SyncRoot)
			{
				GetHandlers(routedEvent).Add(RoutedEventHandler.Cast(handler));
			}
		}

		public void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
		{
			lock (Root.SyncRoot)
			{
				GetHandlers(routedEvent).Remove(RoutedEventHandler.Cast(handler));
			}
		}

		public void AttachParent(RoutedEventTreeNode? node)
		{
			lock (Root.SyncRoot)
			{
				parent?.children.Remove(this);
				parent = node;
				
				if (node is not null)
				{
					lock (node.Root.SyncRoot)
					{
						node.children.Add(this);
					}
				}
			}
		}

		public void OverrideHandlerExecutor(HandlerExecutor? overrideHandlerExecutor = null)
		{
			lock (Root.SyncRoot)
			{
				this.overrideHandlerExecutor = overrideHandlerExecutor;
			}
		}

		public Task Invoke<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, TEventArgs args, HandlerExecutor? overrideExecutor = null) where TEventArgs : notnull, EventArgs
		{
			lock (Root.SyncRoot)
			{
				var output = new List<HandlerInvokable<TEventArgs>>();

				CollectHandlers(routedEvent, output);

				return Task.WhenAll(output.Select(s => s.Invoke(args, overrideExecutor ?? DelegatedHandlerExecutor, this)));
			}
		}

		private void CollectHandlers<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, ICollection<HandlerInvokable<TEventArgs>> output) where TEventArgs : notnull, EventArgs
		{
			var eventHandlers = GetHandlers(routedEvent);
			foreach (var handler in eventHandlers.Select(s => s.As<TEventArgs>()))
			{
				output.Add(new HandlerInvokable<TEventArgs>(handler, routedEvent, this));
			}

			switch (routedEvent.Propagation)
			{
				case RoutedEvent.PropagationDirection.None:
					break;

				case RoutedEvent.PropagationDirection.Bubbling:
					parent?.CollectHandlers(routedEvent, output);
					break;

				case RoutedEvent.PropagationDirection.Tunneling:
					foreach (var child in children)
						child.CollectHandlers(routedEvent, output);
					break;

				default:
					throw new NotSupportedException($"RoutedEventTreeNode doesn't support {routedEvent.Propagation} event propagation type");
			}
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


		private sealed class CompositionRoot
		{
			public object SyncRoot { get; } = new object();
		}


		private record struct HandlerInvokable<TEventArgs>(RoutedEventHandler<TEventArgs> Handler, RoutedEvent<TEventArgs> RoutedEvent, RoutedEventTreeNode OwnerNode) where TEventArgs : notnull, EventArgs
		{
			public Task Invoke(TEventArgs args, HandlerExecutor executor, RoutedEventTreeNode originalSource)
			{
				var handler = Handler;
				var node = OwnerNode;
				var routedEvent = RoutedEvent;

				var sender = new RoutedEventSender(originalSource.owner, OwnerNode.owner, () => { node.owner.RemoveListener(routedEvent, handler); });
				return executor.Invoke(() => handler.Invoke(sender, args));
			}
		}
	}
}
