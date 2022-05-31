namespace DidiFrame.Utils.StateMachine
{
	/// <summary>
	/// Statemachine freeze control object
	/// </summary>
	/// <typeparam name="TState">Type of statemachine state</typeparam>
	/// <param name="DisposeDelegate">Object dispose handler</param>
	public record FreezeModel<TState>(Func<StateUpdateResult<TState>> DisposeDelegate) : IDisposable where TState : struct
	{
		private StateUpdateResult<TState>? result;


		/// <inheritdoc/>
		public void Dispose()
		{
			result = DisposeDelegate.Invoke();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Gets state update result after unfreezing
		/// </summary>
		/// <returns>State update result of owner statemachine</returns>
		/// <exception cref="InvalidOperationException">If objects hasn't diposed yet</exception>
		public StateUpdateResult<TState> GetResult() =>
			result ?? throw new InvalidOperationException("Dispose object before get result");
	}
}
