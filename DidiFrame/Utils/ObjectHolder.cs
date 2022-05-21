namespace DidiFrame.Utils
{
	/// <summary>
	/// Safe container for objects
	/// </summary>
	/// <typeparam name="TObject">Type of internal object</typeparam>
	public class ObjectHolder<TObject> : IDisposable
	{
		private readonly Action<ObjectHolder<TObject>> callback;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.ObjectHolder`1
		/// </summary>
		/// <param name="obj">Wraping object</param>
		/// <param name="callback">Callback on dispose</param>
		public ObjectHolder(TObject obj, Action<ObjectHolder<TObject>> callback)
		{
			Object = obj;
			this.callback = callback;
		}


		/// <summary>
		/// Wrapped object
		/// </summary>
		public TObject Object { get; }


		public void Dispose()
		{
			callback(this);
			GC.SuppressFinalize(this);
		}
	}
}
