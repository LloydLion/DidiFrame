namespace DidiFrame.Utils.RoutedEvents
{
	public interface IRoutedEventObject
	{
		public void AddListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs;

		public void RemoveListener<TEventArgs>(RoutedEvent<TEventArgs> routedEvent, RoutedEventHandler<TEventArgs> handler) where TEventArgs : notnull, EventArgs;
	}
}
