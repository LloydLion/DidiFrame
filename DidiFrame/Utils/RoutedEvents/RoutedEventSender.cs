namespace DidiFrame.Utils.RoutedEvents
{
	public class RoutedEventSender
	{
		private readonly UnSubscriber unSubscriber;


		public RoutedEventSender(IRoutedEventObject originalSource, IRoutedEventObject sender, UnSubscriber unSubscriber)
		{
			OriginalSource = originalSource;
			Sender = sender;
			this.unSubscriber = unSubscriber;
		}


		public IRoutedEventObject OriginalSource { get; }

		public IRoutedEventObject Sender { get; }


		public void UnSubscribeMyself()
		{
			unSubscriber.UnSubscribe();
		}


		public class UnSubscriber
		{
			private readonly Action<dynamic> unSubMethod;
			private dynamic? parameter;


			public UnSubscriber(Action<dynamic> unSubMethod)
			{
				this.unSubMethod = unSubMethod;
			}


			public void SetParameter(dynamic? parameter) => this.parameter = parameter;

			public void UnSubscribe()
			{
				unSubMethod(parameter ?? throw new NotSupportedException());
			}
		}
	}
}
