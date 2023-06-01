namespace DidiFrame.Utils.RoutedEvents
{
	public delegate void RoutedEventHandler<in TEventArgs>(RoutedEventSender sender, TEventArgs args) where TEventArgs : notnull, EventArgs;
}
