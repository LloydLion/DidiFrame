namespace DidiFrame.Utils.RoutedEvents
{
	public delegate void RoutedEventHandler<in TEventArgs>(RoutedEventSender sender, TEventArgs args) where TEventArgs : notnull, EventArgs;


	public class RoutedEventHandler
	{
		private readonly object handler;


		private RoutedEventHandler(object handler)
		{
			this.handler = handler;
		}


		public static RoutedEventHandler Cast<TEventArgs>(RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs
		{
			return new RoutedEventHandler(handler);
		}

		public RoutedEventHandler<TEventArgs> As<TEventArgs>() where TEventArgs : notnull, EventArgs
		{
			return (RoutedEventHandler<TEventArgs>)handler;
		}

		public override bool Equals(object? obj) => handler.Equals(obj);

		public override int GetHashCode() => handler.GetHashCode();
	}
}
