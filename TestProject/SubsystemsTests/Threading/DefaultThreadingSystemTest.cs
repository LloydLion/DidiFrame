using DidiFrame.Threading;
using Microsoft.Extensions.Logging;
using System;

namespace TestProject.SubsystemsTests.Threading
{
	public class DefaultThreadingSystemTest : IThreadingSystemTest<DefaultThreadingSystem>
	{
		protected override DefaultThreadingSystem CreateNewSystem()
		{
			return new DefaultThreadingSystem(new Logger<DefaultThreadingSystem>());
		}


		private class Logger<T> : ILogger<T>
		{
			public IDisposable BeginScope<TState>(TState state)
			{
				return EmptyDisposable.Instance;
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				return false;
			}

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
			{
				// Method intentionally left empty.
			}


			private class EmptyDisposable : IDisposable
			{
				public static IDisposable Instance { get; } = new EmptyDisposable();


				private EmptyDisposable()
				{

				}


				public void Dispose()
				{
					// Method intentionally left empty.
				}
			}

		}
	}
}
