namespace DidiFrame.Utils.StateMachine
{
	public record FreezeModel<TState>(Func<StateUpdateResult<TState>> DisposeDelegate) : IDisposable where TState : struct
	{
		private StateUpdateResult<TState>? result;


		public void Dispose()
		{
			result = DisposeDelegate.Invoke();
			GC.SuppressFinalize(this);
		}

		public StateUpdateResult<TState> GetResult() =>
			result ?? throw new InvalidOperationException("Dispose object before get result");
	}
}
