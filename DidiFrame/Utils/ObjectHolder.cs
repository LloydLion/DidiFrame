namespace DidiFrame.Utils
{
	/// <summary>
	/// Safe container for objects
	/// </summary>
	/// <typeparam name="TObject">Type of internal object</typeparam>
	public class ObjectHolder<TObject> : IDisposable where TObject : class
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


		/// <inheritdoc/>
		public void Dispose()
		{
			callback(this);
			GC.SuppressFinalize(this);
		}

		public TSelect SelectAndClose<TSelect>(Func<TObject, TSelect> selector)
		{
			var value = selector(Object);
			Dispose();
			return value;
		}

		public ObjectHolder<TSelect> SelectHolder<TSelect>(Func<TObject, TSelect> selector) where TSelect : class
		{
			return new ObjectHolder<TSelect>(selector(Object), _ => Dispose());
		}
	}
}
