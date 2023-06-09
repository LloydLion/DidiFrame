namespace DidiFrame.Utils.RoutedEvents
{
	public struct RoutedEventSender
	{
		private readonly Action unSubscribeDelegate;


		public RoutedEventSender(IRoutedEventObject originalSource, IRoutedEventObject sender, Action unSubscribeDelegate)
		{
			OriginalSource = originalSource;
			Sender = sender;
			this.unSubscribeDelegate = unSubscribeDelegate;
		}


		public IRoutedEventObject OriginalSource { get; }

		public IRoutedEventObject Sender { get; }


		public void UnSubscribeMyself()
		{
			unSubscribeDelegate();
		}
	}
}
