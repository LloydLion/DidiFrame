namespace DidiFrame.Utils
{
	public class CompositeAsyncDisposable : IAsyncDisposable
	{
		private readonly HashSet<IAsyncDisposable> disposables = new();
		private bool isDisposed = false;


		public void PushDisposable(IAsyncDisposable disposable)
		{
			disposables.Add(disposable);
		}


		public async ValueTask DisposeAsync()
		{
			if (isDisposed)
				throw new ObjectDisposedException(nameof(CompositeAsyncDisposable));

			isDisposed = true;

			var tasks = disposables.Select(s => s.DisposeAsync()).ToArray();

			foreach (var task in tasks)
				await task;
		}
	}
}
