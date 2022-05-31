namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Abstract implementation of DidiFrame.Utils.StateMachine.IStateTransitWorker`1
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	public abstract class AbstractTransitWorker<TState> : IStateTransitWorker<TState> where TState : struct
	{
		private readonly TState activation;
		private readonly TState? destination;


		/// <summary>
		/// Owner-statemachine
		/// </summary>
		public IStateMachine<TState>? StateMachine { get; private set; }


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.StateMachine.AbstractTransitWorker`1
		/// </summary>
		/// <param name="activation">Activation state</param>
		/// <param name="destination">Target state or nul to finish statemachine</param>
		public AbstractTransitWorker(TState activation, TState? destination)
		{
			this.activation = activation;
			this.destination = destination;
		}


		/// <inheritdoc/>
		public bool CanActivate(TState state) => activation.Equals(state);

		/// <inheritdoc/>
		public TState? DoTransit() => destination;

		/// <inheritdoc/>
		public void Activate(IStateMachine<TState> stateMachine)
		{
			StateMachine = stateMachine;
			Activate();
		}

		/// <summary>
		/// Activates transit, statemachine calls it when activation state has reached
		/// </summary>
		public abstract void Activate();

		/// <summary>
		/// Periodicly calling by state machine after activataion util this or some other transit return true
		/// </summary>
		/// <returns></returns>
		public abstract bool CanDoTransit();

		/// <summary>
		/// Disactivates transit, statemachine calls it when some (this or other) transit switches statemachine state from active state
		/// </summary>
		public abstract void Disactivate();
	}
}
