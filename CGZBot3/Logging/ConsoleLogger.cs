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
		private static DateTime start;
		private static bool init;


		private Stack<ScopeHandler> scopeStack = new();


		public ConsoleLogger(string categoryName, Format format)
		{
			this.categoryName = categoryName;
			console = format;
			if (init == false)
			{
				console.WriteLine($"Startup - now: {DateTime.Now}", Colors.txtInfo);
				start = DateTime.Now;
				init = true;
			}
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
			if (scopeStack.Count != 0) scope = '|' + scope;

			console.Write("[Day:");
			console.Write($"{(DateTime.Now - start).Days} {DateTime.Now:HH:mm:ss}", Colors.txtPrimary);
			console.Write("] [");
			console.Write($"{categoryName}|{eventId.Name}({eventId.Id}){scope}", Colors.txtSuccess);
			console.Write($"] [");
			console.Write(logLevel.ToString(), logLevel switch
			{
				LogLevel.Trace => Colors.bgMuted,
				LogLevel.Debug => Colors.bgMuted,
				LogLevel.Information => Colors.bgInfo,
				LogLevel.Warning => Colors.bgWarning,
				LogLevel.Error => Colors.bgDanger,
				LogLevel.Critical => Colors.bgDanger,
				_ or LogLevel.None => Colors.txtDefault,
			});
			console.Write($"] : {msg}");

			Console.WriteLine();
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
