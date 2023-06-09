namespace DidiFrame.Utils.RoutedEvents
{
	public delegate void RoutedEventSyncHandler<in TEventArgs>(RoutedEventSender sender, TEventArgs args) where TEventArgs : notnull, EventArgs;
}
