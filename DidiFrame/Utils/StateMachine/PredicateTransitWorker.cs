namespace DidiFrame.Utils.StateMachine
{
	public class PredicateTransitWorker<TState> : AbstractTransitWorker<TState> where TState : struct
	{
		private static readonly EventId PredicateErrorID = new(12, "PredicateError");


		private readonly Func<bool> predicate;


		public PredicateTransitWorker(TState activation, TState? destination, Func<bool> predicate)
			: base(activation, destination)
		{
			this.predicate = predicate;
		}


		public override void Activate() { }

		public override bool CanDoTransit()
		{
			try
			{
				return predicate();
			}
			catch (Exception ex)
			{
				StateMachine?.Logger.Log(LogLevel.Warning, PredicateErrorID, ex, "Exception while executing predicate. Returned false");
				return false;
			}
		}

		public override void Disactivate() { }
	}
}
