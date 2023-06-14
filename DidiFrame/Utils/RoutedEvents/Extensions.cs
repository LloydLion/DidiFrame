using DidiFrame.Threading;

namespace DidiFrame.Utils.RoutedEvents
{
	public static class Extensions
	{
		public static void AddListener<TEventArgs>(this IRoutedEventObject eventObject, RoutedEvent<TEventArgs> routedEvent, RoutedEventSyncHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
		{
			eventObject.AddListener(routedEvent, new HandlerClass<TEventArgs>(handler).HandlerWrap);
		}

		public static void RemoveListener<TEventArgs>(this IRoutedEventObject eventObject, RoutedEvent<TEventArgs> routedEvent, RoutedEventSyncHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
		{
			eventObject.RemoveListener(routedEvent, new HandlerClass<TEventArgs>(handler).HandlerWrap);
		}



		private sealed class HandlerClass<TEventArgs> where TEventArgs : notnull, EventArgs
		{
			private readonly RoutedEventSyncHandler<TEventArgs> handler;


			public HandlerClass(RoutedEventSyncHandler<TEventArgs> handler)
			{
				this.handler = handler;
			}

			public override bool Equals(object? obj)
			{
				return handler.Equals(obj);
			}

			public override int GetHashCode()
			{
				return handler.GetHashCode();
			}

			public ValueTask HandlerWrap(RoutedEventSender sender, TEventArgs args)
			{
				handler.Invoke(sender, args);
				return ValueTask.CompletedTask;
			}
		}
	}
}
