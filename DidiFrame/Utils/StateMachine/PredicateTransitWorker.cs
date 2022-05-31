namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Statemachine transit that based on predicate
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	public class PredicateTransitWorker<TState> : AbstractTransitWorker<TState> where TState : struct
	{
		private static readonly EventId PredicateErrorID = new(12, "PredicateError");


		private readonly Func<bool> predicate;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.StateMachine.PredicateTransitWorker`1 using predicate
		/// </summary>
		/// <param name="activation">Activation state (from)</param>
		/// <param name="destination">Destonation state (to)</param>
		/// <param name="predicate">Predicate that will determine transit behavior</param>
		public PredicateTransitWorker(TState activation, TState? destination, Func<bool> predicate)
			: base(activation, destination)
		{
			this.predicate = predicate;
		}


		/// <inheritdoc/>
		public override void Activate() { }

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public override void Disactivate() { }
	}
}
