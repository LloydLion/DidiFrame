namespace DidiFrame.UserCommands.Pipeline.Services
{
	/// <summary>
	/// Local service that represents a list of disposable and disposes it at the end of pipeline
	/// </summary>
	public sealed class Disposer : IDisposable
	{
		private readonly static EventId DisposeErrorID = new(10, "DisposeError");


		private readonly List<IDisposable> disposables = new();
		private readonly ILogger<Disposer> logger;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.Services.Disposer
		/// </summary>
		/// <param name="services">Service provider with logger</param>
		public Disposer(ILogger<Disposer> logger)
		{
			this.logger = logger;
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
				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, DisposeErrorID, ex, "Exception while disposing {{{Component}}} of type {Type}", item, item.GetType());
				}
		}
	}
}
