namespace CGZBot3.Utils
{
	public class StateCollectionHandler<TCollection> : IAsyncDisposable
	{
		public StateCollectionHandler(ICollection<TCollection> collection, Func<StateCollectionHandler<TCollection>, Task> asyncCallback)
		{
			Collection = collection;
			this.AsyncCallback = asyncCallback;
		}


		public ICollection<TCollection> Collection { get; }
		public Func<StateCollectionHandler<TCollection>, Task> AsyncCallback { get; }


		public async ValueTask DisposeAsync() => await AsyncCallback(this);
	}
}
