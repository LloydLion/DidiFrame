using Colorify;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Logging
{
	internal class ConsoleLogger : ILogger
	{
		private readonly string categoryName;
		private readonly Format console;
		private readonly Stack<ScopeHandler> scopeStack = new();
		private readonly ILoggingFilter filter;
		private static DateOnly start;
		private static bool init;
		private static readonly object syncRoot = new();


		public ConsoleLogger(string categoryName, Format format, DateTime start, ILoggingFilter filter)
		{
			lock (syncRoot)
			{
				this.categoryName = categoryName;
				console = format;

				if (init == false)
				{
					console.WriteLine($"Startup - now: {start}", Colors.txtInfo);
					ConsoleLogger.start = DateOnly.FromDateTime(start);
					init = true;
				}
			}

			this.filter = filter;
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
			if(filter.Filter(typeof(ConsoleLoggerProvider).FullName ?? throw new ImpossibleVariantException(), categoryName, logLevel) == false) return;

			var msg = formatter(state, exception);

			var scope = string.Join('/', scopeStack.Select(s => s.State));

			lock (syncRoot)
			{
				console.Write("[Day:");
				console.Write($"{(DateTime.Now - start.ToDateTime(new TimeOnly(0, 0, 0, 0))).Days} {DateTime.Now:HH:mm:ss}", Colors.txtWarning);
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


		private class ScopeHandler : IDisposable
		{
			private readonly ConsoleLogger owner;


			public ScopeHandler(ConsoleLogger owner, object? state)
			{
				this.owner = owner;
				State = state;
			}


			public bool Disposed { get; private set; } = false;

			public object? State { get; }


			public void Dispose()
			{
				Disposed = true;

				if (owner.scopeStack.Peek() != this)
					throw new InvalidOperationException("Can't dispose the scope before next scoped");

				owner.scopeStack.Pop();
			}
		}
	}
}
