//#define DEADLOCK_DETECTOR

namespace DidiFrame.Utils
{
	/// <summary>
	/// Thread synchronization tool
	/// </summary>
	/// <typeparam name="TLock">Type of synch root objects</typeparam>
	public class ThreadLocker<TLock> where TLock : class
	{
		private readonly List<TLock> locked = new();
		private readonly IEqualityComparer<TLock> comparer;
#if DEBUG && DEADLOCK_DETECTOR
		private readonly Dictionary<TLock, DebugInfo> debug = new();
#endif


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
			this.comparer = comparer;
		}


		/// <summary>
		/// Wait for object unlock
		/// </summary>
		/// <param name="obj">Object that need wait</param>
		/// <returns>Wait task</returns>
		public async Task AwaitUnlock(TLock obj)
		{
			while (true)
			{
				lock (locked) { if (locked.Contains(obj) == false) return; }
				await Task.Delay(50);
			}
		}

		/// <summary>
		/// Locks objects if it has already locked waits for unlock
		/// </summary>
		/// <param name="obj">Object that need lock</param>
		/// <returns>Disposable object that should dispose when need to release object</returns>
		public IDisposable Lock(TLock obj)
		{
			AwaitUnlock(obj).Wait();

			lock (locked)
			{
				locked.Add(obj);
#if DEBUG && DEADLOCK_DETECTOR
				var stack = new StackTrace();
				debug.Add(obj, new DebugInfo(stack, stack.GetHashCode(), Thread.CurrentThread, Task.CurrentId));
#endif
			}

			return new Hanlder(this, obj);
		}

		private void Unlock(TLock obj)
		{
			lock (locked)
			{
				locked.RemoveAll(s => comparer.Equals(obj, s));
#if DEBUG && DEADLOCK_DETECTOR
			debug.Remove(obj);
#endif
			}
		}


#if DEBUG && DEADLOCK_DETECTOR
		private record DebugInfo(StackTrace Trace, int StackHash, Thread Thread, int? Task);
#endif

		private class Hanlder : IDisposable
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
	}
}
