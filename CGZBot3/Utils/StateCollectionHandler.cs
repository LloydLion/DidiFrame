namespace CGZBot3.Utils
{
	public class StateCollectionHandler<TCollection> : IAsyncDisposable
	{
		private readonly Func<StateCollectionHandler<TCollection>, Task> asyncCallback;


		public StateCollectionHandler(ICollection<TCollection> collection, Func<StateCollectionHandler<TCollection>, Task> asyncCallback)
		{
			Collection = collection;
			this.asyncCallback = asyncCallback;
		}


		public ICollection<TCollection> Collection { get; }


		public async ValueTask DisposeAsync() => await asyncCallback(this);
	}
}
