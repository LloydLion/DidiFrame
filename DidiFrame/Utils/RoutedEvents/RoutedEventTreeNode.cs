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
		private readonly int threadId;


		public RoutedEventTreeNode(IRoutedEventObject owner, int? targetThread = null, HandlerExecutor? overrideHandlerExecutor = null)
		{
			threadId = targetThread ?? Environment.CurrentManagedThreadId;
			this.owner = owner;
			this.overrideHandlerExecutor = overrideHandlerExecutor;
		}


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
			if (Environment.CurrentManagedThreadId != threadId)
				throw new ThreadAccessException(nameof(RoutedEventTreeNode), 0, nameof(AttachParent));

			if (node is not null && Environment.CurrentManagedThreadId != node.threadId)
				throw new ThreadAccessException(nameof(RoutedEventTreeNode), 0, nameof(AttachParent));

			parent?.children.Remove(this);
			parent = node;
			parent?.children.Add(this);
		}

		public void OverrideHandlerExecutor(HandlerExecutor? overrideHandlerExecutor = null)
		{
			if (Environment.CurrentManagedThreadId != threadId)
				throw new ThreadAccessException(nameof(RoutedEventTreeNode), 0, nameof(OverrideHandlerExecutor));

			this.overrideHandlerExecutor = overrideHandlerExecutor;
		}

		public Task Invoke<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, TEventArgs args, HandlerExecutor? overrideExecutor = null) where TEventArgs : notnull, EventArgs
		{
			if (Environment.CurrentManagedThreadId != threadId)
				throw new ThreadAccessException(nameof(RoutedEventTreeNode), 0, nameof(Invoke));

			var output = new List<HandlerInvokable<TEventArgs>>();

			lock (Root.SyncRoot)
			{
				CollectHandlers(routedEvent, output);
			}

			return Task.WhenAll(output.Select(s => s.Invoke(args, overrideExecutor ?? DelegatedHandlerExecutor, this)));
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
