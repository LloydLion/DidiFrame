namespace DidiFrame.Utils
{
	/// <summary>
	/// Useful tool for wait some events
	/// </summary>
	public class WaitFor
	{
		private TaskCompletionSource tcs = new();


		/// <summary>
		/// If wait complited
		/// </summary>
		public bool WaitDone => tcs.Task.IsCompleted;


		/// <summary>
		/// Complites wait task
		/// </summary>
		public void Callback() => tcs.SetResult();

		/// <summary>
		/// Provides wait task
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public Task Await(CancellationToken token = default) => tcs.Task.WaitAsync(token);

		/// <summary>
		/// Resets object and wait task
		/// </summary>
		public void Reset() => tcs = new();
	}
}
