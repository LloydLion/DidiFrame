using Microsoft.Extensions.Logging;

namespace DidiFrame.Testing.Logging
{
	/// <summary>
	/// Test ILogger`1 implmentation
	/// </summary>
	/// <typeparam name="TType">Target logging type</typeparam>
	public class DebugConsoleLogger<TType> : DebugConsoleLogger, ILogger<TType>
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Testing.Logging.DebugConsoleLogger`1
		/// </summary>
		/// <param name="minLevel">Min logging level</param>
		public DebugConsoleLogger(LogLevel minLevel = LogLevel.None) : base(typeof(TType).FullName ?? "NONAME", minLevel)
		{

		}
	}

	/// <summary>
	/// Test ILogger implmentation
	/// </summary>
	public class DebugConsoleLogger : ILogger
	{
		private readonly string categoryName;
		private readonly LogLevel minLevel;
		private readonly Stack<object?> scopes = new();


		/// <summary>
		/// Creates new instance of DidiFrame.Testing.Logging.DebugConsoleLogger
		/// </summary>
		/// <param name="categoryName">Name of category</param>
		/// <param name="minLevel">Min logging level</param>
		protected DebugConsoleLogger(string categoryName, LogLevel minLevel)
		{
			this.categoryName = categoryName;
			this.minLevel = minLevel;
		}


		/// <inheritdoc/>
		public IDisposable BeginScope<TState>(TState state)
		{
			return new ScopeHandler(this, state);
		}

		/// <inheritdoc/>
		public bool IsEnabled(LogLevel logLevel)
		{
			return minLevel <= logLevel;
		}

		/// <inheritdoc/>
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			var text = formatter(state, exception);
			var scope = scopes.Count == 0 ? "" : "|" + string.Join(", ", scopes);

			Console.WriteLine($"({logLevel.ToString().ToUpper()}) | [{DateTime.Now.ToShortTimeString()}] [{categoryName}{scope}] [{eventId.Name}({eventId.Id})]: {text}");
		}


		private sealed class ScopeHandler : IDisposable
		{
			private readonly DebugConsoleLogger logger;


			public ScopeHandler(DebugConsoleLogger logger, object? scopeObject)
			{
				this.logger = logger;
				logger.scopes.Push(scopeObject);
			}


			public void Dispose()
			{
				logger.scopes.Pop();
			}
		}
	}
}
