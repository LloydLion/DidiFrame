namespace CGZBot3.Utils
{
	internal class WaitFor
	{
		private bool waitDone = false;


		public bool WaitDone => waitDone;


		public void Callback() => waitDone = true;

		public async Task Await(CancellationToken token = default)
		{
			while (!waitDone && !token.IsCancellationRequested) await Task.Delay(50, token);
		}

		public void Reset() => waitDone = false;
	}
}
