namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Simple transit router that has one activation state and one destonation
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	public class SimpleTranitRouter<TState> : IStateTransitRouter<TState> where TState : struct
	{
		private readonly object startState; //Prevent pack process
		private readonly TState? destonation;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.StateMachine.SimpleTranitRouter`1
		/// </summary>
		/// <param name="startState">Activation state</param>
		/// <param name="destonation">Destonation state</param>
		public SimpleTranitRouter(TState startState, TState? destonation)
		{
			this.startState = startState;
			this.destonation = destonation;
		}


		/// <inheritdoc/>
		public bool CanActivate(TState state) => state.Equals(startState);

		/// <inheritdoc/>
		public TState? SwitchState(TState oldState) => destonation;
	}
}
