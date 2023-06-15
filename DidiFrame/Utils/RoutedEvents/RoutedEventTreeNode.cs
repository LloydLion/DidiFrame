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
		private TaskCompletionSource? invokeWaitTask = null;


		public RoutedEventTreeNode(IRoutedEventObject owner, HandlerExecutor? overrideHandlerExecutor = null)
		{
			this.owner = owner;
			this.overrideHandlerExecutor = overrideHandlerExecutor;
		}


		private HandlerExecutor? DelegatedHandlerExecutor
		{
			get
			{
				return overrideHandlerExecutor ?? parent?.DelegatedHandlerExecutor;
			}
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

		public void OverrideHandlerExecutor(HandlerExecutor? overrideHandlerExecutor = null)
		{
			this.overrideHandlerExecutor = overrideHandlerExecutor;
		}

		public async Task Invoke<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, TEventArgs args, HandlerExecutor? overrideExecutor = null) where TEventArgs : notnull, EventArgs
		{
			if (invokeWaitTask is not null)
				await invokeWaitTask.Task;

			invokeWaitTask = new();

			var saved = overrideHandlerExecutor;
			overrideHandlerExecutor = overrideExecutor ?? saved;

			await InvokeInternal(routedEvent, args, owner);

			overrideHandlerExecutor = saved;

			invokeWaitTask = null;
		}

		private async Task InvokeInternal<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, TEventArgs args, IRoutedEventObject originalSource, HandlerExecutor? prevExecutor = null) where TEventArgs : notnull, EventArgs
		{
			var executor = DelegatedHandlerExecutor ?? prevExecutor ?? throw new InvalidOperationException("No handler executor in routed event tree");

			var eventHandlers = GetHandlers(routedEvent);

			var tasks = new List<Task>();
			foreach (var handler in eventHandlers.Select(s => s.As<TEventArgs>()))
			{
				var sender = new RoutedEventSender(originalSource, owner, () => { owner.RemoveListener(routedEvent, handler); });
				tasks.Add(executor.Invoke(() => handler.Invoke(sender, args)));
			}

			switch (routedEvent.Propagation)
			{
				case RoutedEvent.PropagationDirection.None:
					break;

				case RoutedEvent.PropagationDirection.Bubbling:
					if (parent is not null)
						await parent.InvokeInternal(routedEvent, args, owner, executor);
					break;

				case RoutedEvent.PropagationDirection.Tunneling:
					await Task.WhenAll(children.Select(child => child.InvokeInternal(routedEvent, args, owner, executor)));
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
