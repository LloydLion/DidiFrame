namespace DidiFrame.Utils
{
	/// <summary>
	/// Useful tool for wait some events
	/// </summary>
	public class WaitFor
	{
		private bool waitDone = false;


		/// <summary>
		/// If wait complited
		/// </summary>
		public bool WaitDone => waitDone;


		/// <summary>
		/// Complites wait task
		/// </summary>
		public void Callback() => waitDone = true;

		/// <summary>
		/// Provides wait task
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task Await(CancellationToken token = default)
		{
			while (!waitDone && !token.IsCancellationRequested) await Task.Delay(50, token);
		}

		/// <summary>
		/// Resets object and wait task
		/// </summary>
		public void Reset() => waitDone = false;
	}
}
