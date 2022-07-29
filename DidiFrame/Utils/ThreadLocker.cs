//#define DEADLOCK_DETECTOR

using System.Collections.Concurrent;

namespace DidiFrame.Utils
{
	/// <summary>
	/// Thread synchronization tool
	/// </summary>
	/// <typeparam name="TLock">Type of synch root objects</typeparam>
	public class ThreadLocker<TLock> where TLock : notnull
	{
		private readonly ConcurrentDictionary<TLock, AutoResetEvent> locked;


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
			locked.GetOrAdd(obj, _ => new(true)).WaitOne();

			return new Hanlder(this, obj);
		}

		/// <summary>
		/// Gets auto reset event from lock object that synchronized with locker
		/// </summary>
		/// <param name="obj">Target object</param>
		/// <returns>Synchronized auto reset event</returns>
		public AutoResetEvent GetLockObject(TLock obj) => locked.GetOrAdd(obj, _ => new(true));

		private void Unlock(TLock obj)
		{
			locked.GetOrAdd(obj, _ => new(true)).Set();
		}


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
