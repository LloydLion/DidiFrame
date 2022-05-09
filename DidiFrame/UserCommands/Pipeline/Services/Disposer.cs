namespace DidiFrame.UserCommands.Pipeline.Services
{
	public class Disposer : IDisposable
	{
		private readonly List<IDisposable> disposables = new();



		public void AddDisposable(IDisposable toDispose)
		{
			disposables.Add(toDispose);
		}


		public void Dispose()
		{
			GC.SuppressFinalize(this);

			foreach (var item in disposables)
				try { item.Dispose(); }
				catch (Exception) { }
		}
	}
}
