namespace CGZBot3.Utils
{
	public class StateHandler<TObject> : IAsyncDisposable
	{
		private readonly Func<StateHandler<TObject>, Task> asyncCallback;


		public StateHandler(TObject obj, Func<StateHandler<TObject>, Task> asyncCallback)
		{
			Object = obj;
			this.asyncCallback = asyncCallback;
		}


		public TObject Object { get; }


		public async ValueTask DisposeAsync()
		{
			await asyncCallback(this);
			GC.SuppressFinalize(this);
		}
	}
}
