namespace CGZBot3.Utils
{
	internal class ThreadLocker<TLock> where TLock : class
	{
		private readonly List<TLock> locked = new();
		private readonly IEqualityComparer<TLock> comparer;


		public ThreadLocker() : this(EqualityComparer<TLock>.Default) { }

		public ThreadLocker(IEqualityComparer<TLock> comparer)
		{
			this.comparer = comparer;
		}


		public async Task AwaitUnlock(TLock obj)
		{
			while (locked.Contains(obj, comparer)) { await Task.Delay(50); }
		}

		public IDisposable Lock(TLock obj)
		{
			AwaitUnlock(obj).Wait();
			locked.Add(obj);
			return new Hanlder(this, obj);
		}

		private void Unlock(TLock obj)
		{
			locked.RemoveAll(s => comparer.Equals(obj, s));
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
