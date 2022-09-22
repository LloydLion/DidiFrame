namespace DidiFrame.Utils
{
	/// <summary>
	/// Thread synchronization tool
	/// </summary>
	/// <typeparam name="TLock">Type of synch root objects</typeparam>
	public sealed class ThreadLocker<TLock> where TLock : notnull
	{
		private readonly Dictionary<TLock, LockItem> locked;


		/// <summary>
		/// Creates DidiFrame.Utils.ThreadLocker`1 using default equality comparer for TLock
		/// </summary>
		public ThreadLocker() : this(EqualityComparer<TLock>.Default) { }

		/// <summary>
		/// Creates DidiFrame.Utils.ThreadLocker`1 using given equality comparer
		/// </summary>
		/// <param name="comparer">Some equality comparer for TLock</param>
		public ThreadLocker(IEqualityComparer<TLock> comparer)
		{
			locked = new(comparer);
		}


		/// <summary>
		/// Locks objects if it has already locked waits for unlock
		/// </summary>
		/// <param name="obj">Object that need lock</param>
		/// <returns>Disposable object that should dispose when need to release object</returns>
		public IDisposable Lock(TLock obj)
		{
			AutoResetEvent resetEvent;

			lock (locked)
			{
				if (locked.ContainsKey(obj))
				{
					var item = locked[obj];
					resetEvent = item.ResetEvent;
					item.WaitersCount++;
					locked[obj] = item;
				}
				else
				{
					locked.Add(obj, new(new(initialState: false)));
					return new Hanlder(this, obj);
				}
			}

			resetEvent.WaitOne();
			return new Hanlder(this, obj);
		}

		private void Unlock(TLock obj)
		{
			lock (locked)
			{
				var item = locked[obj];

				if (item.WaitersCount == 0)
					locked.Remove(obj);
				else
				{
					item.WaitersCount--;
					locked[obj] = item;
				}

				item.ResetEvent.Set();
			}
		}


		/// <summary>
		/// Agent for thread locker that bakes object to lock
		/// </summary>
		public sealed class Agent
		{
			private readonly ThreadLocker<TLock> locker;
			private readonly TLock objectToLock;


			/// <summary>
			/// Creates new instance of DidiFrame.Utils.ThreadLocker`1.Agent
			/// </summary>
			/// <param name="locker">Base locker object</param>
			/// <param name="objectToLock">Object that will be locked</param>
			public Agent(ThreadLocker<TLock> locker, TLock objectToLock)
			{
				this.locker = locker;
				this.objectToLock = objectToLock;
			}


			/// <summary>
			/// Locks agent
			/// </summary>
			/// <returns>Disposable object that should dispose when need to release object</returns>
			public IDisposable Lock() => locker.Lock(objectToLock);
		}

		private sealed class Hanlder : IDisposable
		{
			private readonly ThreadLocker<TLock> owner;
			private readonly TLock obj;


			public Hanlder(ThreadLocker<TLock> owner, TLock obj)
			{
				this.owner = owner;
				this.obj = obj;
			}


			public void Dispose()
			{
				owner.Unlock(obj);
			}
		}

		private struct LockItem
		{
			public LockItem(AutoResetEvent resetEvent)
			{
				ResetEvent = resetEvent;
				WaitersCount = 0;
			}


			public AutoResetEvent ResetEvent { get; }
		
			public int WaitersCount { get; set; }
		}
	}
}
