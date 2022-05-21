namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Builder for DidiFrame.Utils.StateMachine.StateMachine`1
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	public class StateMachineBuilder<TState> where TState : struct
	{
		private readonly ILogger logger;
		private readonly List<IStateTransitWorker<TState>> workers = new();
		private readonly List<StateChangedEventHandler<TState>> handlers = new();


		/// <summary>
		/// Creates DidiFrame.Utils.StateMachine.StateMachineBuilder`1 instance
		/// </summary>
		/// <param name="logger">Logger for statemachine</param>
		public StateMachineBuilder(ILogger logger)
		{
			this.logger = logger;
		}


		/// <summary>
		/// Adds transit worker into machine
		/// </summary>
		/// <param name="worker"></param>
		public void AddStateTransitWorker(IStateTransitWorker<TState> worker) => workers.Add(worker);

		/// <summary>
		/// Adds state changed event handler into machine
		/// </summary>
		/// <param name="handler">Handler itself</param>
		public void AddStateChangedHandler(StateChangedEventHandler<TState> handler) => handlers.Add(handler);

		/// <summary>
		/// Builds statemahcine
		/// </summary>
		/// <returns>New statemachine instance</returns>
		public StateMachine<TState> Build()
		{
			var sm = new StateMachine<TState>(workers, logger);
			foreach (var handler in handlers) sm.StateChanged += handler;

			return sm;
		}
	}
}
