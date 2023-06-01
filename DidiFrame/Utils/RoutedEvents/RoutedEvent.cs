using static DidiFrame.Utils.RoutedEvents.RoutedEvent;

namespace DidiFrame.Utils.RoutedEvents
{
	public class RoutedEvent<TEventArgs> where TEventArgs : notnull, EventArgs
	{
		public RoutedEvent(Type eventOwner, string name, PropagationDirection propagation = PropagationDirection.Bubbling)
		{
			EventOwner = eventOwner;
			Name = name;
			Propagation = propagation;
		}


		public PropagationDirection Propagation { get; }

		public Type EventOwner { get; }

		public string Name { get; }

	}

	public static class RoutedEvent
	{
		public enum PropagationDirection
		{
			None,
			Bubbling,
			Tunneling
		}	
	}
}
