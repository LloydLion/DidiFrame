namespace CGZBot3.Utils
{
	public class ObjectHolder<TObject> : IDisposable
	{
		private readonly Action<ObjectHolder<TObject>> callback;


		public ObjectHolder(TObject obj, Action<ObjectHolder<TObject>> callback)
		{
			Object = obj;
			this.callback = callback;
		}


		public TObject Object { get; }


		public void Dispose()
		{
			callback(this);
			GC.SuppressFinalize(this);
		}
	}
}
