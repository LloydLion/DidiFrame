namespace DidiFrame.UserCommands.Pipeline.Services
{
	/// <summary>
	/// Local service that represents a list of disposable and disposes it at the end of pipeline
	/// </summary>
	public class Disposer : IDisposable
	{
		private readonly List<IDisposable> disposables = new();


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.Services.Disposer
		/// </summary>
		/// <param name="_">Unused service provider</param>
		public Disposer(IServiceProvider? _)
		{

		}


		/// <summary>
		/// Adds disposable object to list to dispose it at end of pipeline
		/// </summary>
		/// <param name="toDispose">Object to dispose</param>
		public void AddDisposable(IDisposable toDispose)
		{
			disposables.Add(toDispose);
		}


		/// <inheritdoc/>
		public void Dispose()
		{
			GC.SuppressFinalize(this);

			foreach (var item in disposables)
				try { item.Dispose(); }
				catch (Exception) { }
		}
	}
}
