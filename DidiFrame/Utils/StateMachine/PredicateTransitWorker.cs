using static DidiFrame.Utils.StateMachine.PredicateTransitWorkerStatic;

namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Statemachine transit that based on predicate
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	public sealed class PredicateTransitWorker<TState> : IStateTransitWorker<TState> where TState : struct
	{
		private readonly Func<bool> predicate;
		private IStateMachine<TState>? stateMachine;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.StateMachine.PredicateTransitWorker`1 using predicate
		/// </summary>
		/// <param name="predicate">Predicate that will determine transit behavior</param>
		public PredicateTransitWorker(Func<bool> predicate)
		{
			this.predicate = predicate;
		}


		/// <inheritdoc/>
		public void Activate(IStateMachine<TState> stateMachine)
		{
			this.stateMachine = stateMachine;
		}

		/// <inheritdoc/>
		public bool CanDoTransit()
		{
			try
			{
				return predicate();
			}
			catch (Exception ex)
			{
				stateMachine?.Logger.Log(LogLevel.Warning, PredicateErrorID, ex, "Exception while executing predicate. Returned false");
				return false;
			}
		}

		/// <inheritdoc/>
		public void Disactivate()
		{
			//Do nothing on disactivate
		}

		/// <inheritdoc/>
		public void DoTransit()
		{
			//Do nothing on transit
		}
	}

	internal static class PredicateTransitWorkerStatic
	{
		public static readonly EventId PredicateErrorID = new(12, "PredicateError");
	}
}
