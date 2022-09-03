using Microsoft.Extensions.Logging;

namespace DidiFrame.Testing.Logging
{
	public class DebugConsoleLogger<TType> : DebugConsoleLogger, ILogger<TType>
	{
		public DebugConsoleLogger(LogLevel minLevel = LogLevel.None) : base(typeof(TType).FullName ?? "NONAME", minLevel)
		{

		}
	}

	public class DebugConsoleLogger : ILogger
	{
		private readonly string categoryName;
		private readonly LogLevel minLevel;
		private readonly Stack<object?> scopes = new();


		protected DebugConsoleLogger(string categoryName, LogLevel minLevel)
		{
			this.categoryName = categoryName;
			this.minLevel = minLevel;
		}


		public IDisposable BeginScope<TState>(TState state)
		{
			return new ScopeHandler(this, state);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return minLevel <= logLevel;
		}

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
