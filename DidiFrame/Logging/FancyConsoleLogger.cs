using Colorify;

namespace DidiFrame.Logging
{
	internal sealed class FancyConsoleLogger : ILogger
	{
		private readonly string categoryName;
		private readonly Format console;
		private readonly Stack<ScopeHandler> scopeStack = new();
		private readonly DateOnly startTime;
		private static readonly object syncRoot = new();

		public FancyConsoleLogger(string categoryName, Format format, DateOnly startTime)
		{
			lock (syncRoot)
			{
				this.categoryName = categoryName;
				console = format;
			}

			this.startTime = startTime;
		}


		public IDisposable BeginScope<TState>(TState state)
		{
			var idis = new ScopeHandler(this, state);
			scopeStack.Push(idis);
			return idis;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			var msg = formatter(state, exception);

			var scope = string.Join('/', scopeStack.Select(s => s.State));

			lock (syncRoot)
			{
				console.Write("[Day:");
				console.Write($"{(DateTime.Now - startTime.ToDateTime(new TimeOnly(0, 0, 0, 0))).Days} {DateTime.Now:HH:mm:ss}", Colors.txtWarning);
				console.Write("] [");
				console.Write(categoryName, Colors.txtSuccess);
				if (scopeStack.Count != 0)
				{
					console.Write("|", Colors.txtDefault);
					console.Write(scope.ToString(), Colors.txtSuccess);
				}
				console.Write("|", Colors.txtDefault);
				console.Write($"{eventId.Name}({eventId.Id})", Colors.txtSuccess);
				console.Write($"] [");
				console.Write(logLevel.ToString(), logLevel switch
				{
					LogLevel.Trace => Colors.bgMuted,
					LogLevel.Debug => Colors.bgPrimary,
					LogLevel.Information => Colors.bgInfo,
					LogLevel.Warning => Colors.bgWarning,
					LogLevel.Error => Colors.bgDanger,
					LogLevel.Critical => Colors.bgDanger,
					_ or LogLevel.None => Colors.txtDefault,
				});
				console.Write($"] : {msg}");

				Console.WriteLine();

				if (exception is not null)
				{
					console.WriteLine(exception.ToString(), Colors.txtDanger);
				}
			}
		}


		private sealed class ScopeHandler : IDisposable
		{
			private readonly FancyConsoleLogger owner;


			public ScopeHandler(FancyConsoleLogger owner, object? state)
			{
				this.owner = owner;
				State = state;
			}


			public bool Disposed { get; private set; } = false;

			public object? State { get; }


			public void Dispose()
			{
				Disposed = true;
				owner.scopeStack.Pop();
			}
		}
	}
}
