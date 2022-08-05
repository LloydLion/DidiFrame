namespace DidiFrame.Utils
{
	/// <summary>
	/// Represents caches of some objects with thier ids
	/// </summary>
	/// <typeparam name="TKey">Type of object's id</typeparam>
	public class ObjectsCache<TKey> where TKey : struct
	{
		private readonly Dictionary<Type, Action<TKey, object>> addHandlers = new();
		private readonly Dictionary<Type, Action<TKey, object>> removeHandlers = new();
		private readonly Dictionary<Type, Frame> frames = new();


		/// <summary>
		/// Sets adding action handler to some object cache
		/// </summary>
		/// <typeparam name="TObject">Type of objects cache</typeparam>
		/// <param name="action">Callback</param>
		public void SetAddActionHandler<TObject>(Action<TKey, TObject> action) where TObject : notnull =>
			addHandlers.Add(typeof(TObject), (key, obj) => action(key, (TObject)obj));

		/// <summary>
		/// Sets removement action handler to some object cache
		/// </summary>
		/// <typeparam name="TObject">Type of objects cache</typeparam>
		/// <param name="action">Callback</param>
		public void SetRemoveActionHandler<TObject>(Action<TKey, TObject> action) where TObject : notnull =>
			removeHandlers.Add(typeof(TObject), (key, obj) => action(key, (TObject)obj));

		/// <summary>
		/// Gets objects caching frame
		/// </summary>
		/// <typeparam name="TObject">Type of objects cahce</typeparam>
		/// <returns>Cache frame</returns>
		public Frame<TObject> GetFrame<TObject>() where TObject : notnull => GetFrameInternal<TObject>();

		/// <summary>
		/// Adds object to some cache. Be careful with TObject
		/// </summary>
		/// <typeparam name="TObject">Type of target cache</typeparam>
		/// <param name="key">Key of object</param>
		/// <param name="obj">Object to add</param>
		public void AddObject<TObject>(TKey key, TObject obj) where TObject : notnull => GetFrameInternal<TObject>().AddObject(key, obj);

		/// <summary>
		/// Deletes object from cache. Be careful with TObject
		/// </summary>
		/// <typeparam name="TObject">Type of target cache</typeparam>
		/// <param name="key">Key of object</param>
		public void DeleteObject<TObject>(TKey key) where TObject : notnull => GetFrameInternal<TObject>().DeleteObject(key);

		private Frame<TObject> GetFrameInternal<TObject>() where TObject : notnull
		{
			if (!frames.ContainsKey(typeof(TObject))) frames.Add(typeof(TObject), new Frame<TObject>(this));
			return (Frame<TObject>)frames[typeof(TObject)];
		}


		/// <summary>
		/// Representation of cache of some objects
		/// </summary>
		/// <typeparam name="TObject"></typeparam>
		public class Frame<TObject> : Frame where TObject : notnull
		{
			internal Frame(ObjectsCache<TKey> owner) : base(owner, typeof(TObject)) { }


			/// <summary>
			/// Adds object to this cache
			/// </summary>
			/// <param name="key">Key of object</param>
			/// <param name="obj">Object to add</param>
			public void AddObject(TKey key, TObject obj) => base.AddObject(key, obj);

			/// <summary>
			/// Gets object from this cache
			/// </summary>
			/// <param name="key">Key of object</param>
			/// <returns>Object from cache</returns>
			public new TObject GetObject(TKey key) => (TObject)base.GetObject(key);

			/// <summary>
			/// Gets object from this cache or null if no object found
			/// </summary>
			/// <param name="key">Key of object</param>
			/// <returns>Object from cache or null if no object found</returns>
			public new TObject? GetNullableObject(TKey key) => (TObject?)base.GetNullableObject(key);

			/// <summary>
			/// Gets all objects that stored in this cache
			/// </summary>
			/// <returns></returns>
			public new IReadOnlyCollection<TObject> GetObjects() => base.GetObjects().Cast<TObject>().ToArray();
		}

		/// <summary>
		/// Representation of cache of some objects
		/// </summary>
		public class Frame
		{
			private readonly Dictionary<TKey, object> objects = new();
			private readonly ObjectsCache<TKey> owner;
			private readonly Type ttype;


			internal Frame(ObjectsCache<TKey> owner, Type ttype)
			{
				this.owner = owner;
				this.ttype = ttype;
			}


			public object SyncRoot { get; } = new();


			/// <summary>
			/// Adds object to this cache
			/// </summary>
			/// <param name="key">Key of object</param>
			/// <param name="obj">Object to add</param>
			public void AddObject(TKey key, object obj)
			{
				lock (SyncRoot)
				{
					var isSuccess = objects.TryAdd(key, obj);
					if (isSuccess && owner.addHandlers.ContainsKey(ttype)) owner.addHandlers[ttype].Invoke(key, obj);
				}
			}

			/// <summary>
			/// Deletes object from cache
			/// </summary>
			/// <param name="key">Key of object</param>
			public void DeleteObject(TKey key)
			{
				lock (SyncRoot)
				{
					if (objects.TryGetValue(key, out var value))
					{
						objects.Remove(key);
						if (owner.removeHandlers.ContainsKey(ttype)) owner.removeHandlers[ttype].Invoke(key, value);
					}
				}
			}

			/// <summary>
			/// Gets object from this cache
			/// </summary>
			/// <param name="key">Key of object</param>
			/// <returns>Object from cache</returns>
			public object GetObject(TKey key)
			{
				lock (SyncRoot)
				{
					return objects[key];
				}
			}

			/// <summary>
			/// Gets object from this cache or null if no object found
			/// </summary>
			/// <param name="key">Key of object</param>
			/// <returns>Object from cache or null if no object found</returns>
			public object? GetNullableObject(TKey key)
			{
				lock (SyncRoot)
				{
					return objects.ContainsKey(key) ? objects[key] : null;
				}
			}

			/// <summary>
			/// Gets all objects that stored in this cache
			/// </summary>
			/// <returns></returns>
			public IReadOnlyCollection<object> GetObjects()
			{
				lock (SyncRoot)
				{
					return objects.Values;
				}
			}

			/// <summary>
			/// Checks if cache contains object with given key
			/// </summary>
			/// <param name="id">Key of object</param>
			/// <returns>If has object</returns>
			public bool HasObject(TKey id)
			{
				lock (SyncRoot)
				{
					return objects.ContainsKey(id);
				}
			}
		}
	}
}
