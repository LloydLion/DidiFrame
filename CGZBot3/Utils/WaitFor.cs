namespace CGZBot3.Utils
{
	internal class WaitFor
	{
		private bool waitDone = false;


		public void Callback() => waitDone = true;

		public async Task Await()
		{
			while (waitDone) await Task.Delay(50);
		}
	}
}
