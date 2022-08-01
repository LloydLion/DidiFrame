namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Event handler delegate for statemachine state changing
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	/// <param name="stateMahcine">Statemachine that called event</param>
	/// <param name="oldState">Old state of statemahcine</param>
	public delegate void StateChangedEventHandler<TState>(IStateMachine<TState> stateMahcine, TState oldState) where TState : struct;


	/// <summary>
	/// Represents finite state machine
	/// </summary>
	/// <typeparam name="TState">Struct that represents state of statemahcine</typeparam>
	public interface IStateMachine<TState> where TState : struct
	{
		/// <summary>
		/// Event that fires when statmachine changing state
		/// </summary>
		public event StateChangedEventHandler<TState>? StateChanged;


		/// <summary>
		/// Current state or null if lifecycle of mahcine has ended
		/// </summary>
		public TState? CurrentState { get; }

		/// <summary>
		/// Logger to log machine actions
		/// </summary>
		public ILogger Logger { get; }


		/// <summary>
		/// Check all conditions and updates state if it need
		/// </summary>
		/// <returns></returns>
		public StateUpdateResult<TState> UpdateState();

		/// <summary>
		/// Starts state machine with given initial state
		/// </summary>
		/// <param name="startState">Initial state</param>
		public void Start(TState startState);

		/// <summary>
		/// Waits for state
		/// </summary>
		/// <param name="state">Target state</param>
		/// <returns>Wait task</returns>
		public Task AwaitForState(TState? state);

		/// <summary>
		/// Freezes all statemachine threads and processes
		/// </summary>
		/// <returns>Freeze control object</returns>
		public FreezeModel<TState> Freeze();
	}
}
