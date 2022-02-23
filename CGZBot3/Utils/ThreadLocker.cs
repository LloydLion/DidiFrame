namespace CGZBot3.Utils
{
	internal class ThreadLocker
	{
		private int locked;


		public async Task AwaitUnlock()
		{
			while (locked != 0) { await Task.Delay(100); }
		}

		public IDisposable Lock()
		{
			locked++;
			return new Hanlder(this);
		}

		private void Unlock()
		{
			locked--;
		}


		private class Hanlder : IDisposable
		{
			private readonly ThreadLocker owner;


			public Hanlder(ThreadLocker owner)
			{
				this.owner = owner;
			}


			public void Dispose()
			{
				owner.Unlock();
			}
		}
	}
}
