namespace DidiFrame.Utils
{
	public class ObjectsCache<TKey> where TKey : struct
	{
		private readonly Dictionary<Type, Action<TKey, object>> addHandlers = new();
		private readonly Dictionary<Type, Action<TKey, object>> removeHandlers = new();
		private readonly Dictionary<Type, Frame> frames = new();


		public ObjectsCache()
		{

		}


		public void SetAddActionHandler<TObject>(Action<TKey, TObject> action) where TObject : notnull =>
			addHandlers.Add(typeof(TObject), (key, obj) => action(key, (TObject)obj));

		public void SetRemoveActionHandler<TObject>(Action<TKey, TObject> action) where TObject : notnull =>
			removeHandlers.Add(typeof(TObject), (key, obj) => action(key, (TObject)obj));

		public Frame<TObject> GetFrame<TObject>() where TObject : notnull => GetFrameInternal<TObject>();

		public void AddObject<TObject>(TKey key, TObject obj) where TObject : notnull => GetFrameInternal<TObject>().AddObject(key, obj);

		public void DeleteObject<TObject>(TKey key) where TObject : notnull => GetFrameInternal<TObject>().DeleteObject(key);

		private Frame<TObject> GetFrameInternal<TObject>() where TObject : notnull
		{
			if (!frames.ContainsKey(typeof(TObject))) frames.Add(typeof(TObject), new Frame<TObject>(this));
			return (Frame<TObject>)frames[typeof(TObject)];
		}


		public class Frame<TObject> : Frame where TObject : notnull
		{
			public Frame(ObjectsCache<TKey> owner) : base(owner, typeof(TObject)) { }


			public void AddObject(TKey key, TObject obj) => base.AddObject(key, obj);

			public new TObject GetObject(TKey key) => (TObject)base.GetObject(key);

			public new TObject? GetNullableObject(TKey key) => (TObject?)base.GetNullableObject(key);

			public IReadOnlyCollection<TObject> GetObjects() => base.GetObjects().Cast<TObject>().ToArray();
		}

		public class Frame
		{
			private readonly Dictionary<TKey, object> objects = new();
			private readonly ObjectsCache<TKey> owner;
			private readonly Type ttype;


			public Frame(ObjectsCache<TKey> owner, Type ttype)
			{
				this.owner = owner;
				this.ttype = ttype;
			}


			public void AddObject(TKey key, object obj)
			{
				lock (this)
				{
					var isSuccess = objects.TryAdd(key, obj);
					if (isSuccess && owner.addHandlers.ContainsKey(ttype)) owner.addHandlers[ttype].Invoke(key, obj);
				}
			}

			public void DeleteObject(TKey key)
			{
				lock (this)
				{
					if (objects.TryGetValue(key, out var value))
					{
						objects.Remove(key);
						if (owner.removeHandlers.ContainsKey(ttype)) owner.removeHandlers[ttype].Invoke(key, value);
					}
				}
			}

			public object GetObject(TKey key)
			{
				lock (this)
				{
					return objects[key];
				}
			}

			public object? GetNullableObject(TKey key)
			{
				lock (this)
				{
					return objects.ContainsKey(key) ? objects[key] : null;
				}
			}

			public IReadOnlyCollection<object> GetObjects()
			{
				lock (this)
				{
					return objects.Values;
				}
			}

			public bool HasObject(TKey id)
			{
				lock (this)
				{
					return objects.ContainsKey(id);
				}
			}
		}
	}
}
