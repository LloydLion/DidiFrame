using DidiFrame.Exceptions;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp.Utils
{
	public class EventBuffer : IServerTask
	{
		private static readonly EventId BufferInitializedID = new(11, "BufferInitialized");
		private static readonly EventId FinishedID = new(12, "Finished");
		private static readonly EventId ExceptionInEventID = new(21, "ExceptionInEvent");


		private readonly TimeSpan size;
		private readonly int syncThread;
		private readonly ILogger<EventBuffer> logger;
		private readonly Timer timer;
		private readonly List<Action> events = new();
		private IServerTaskExecutionContext? context;
		private bool isPerformTerminate = false;
		private Action? onCompleted;


		public EventBuffer(TimeSpan size, int syncThread, ILogger<EventBuffer> logger)
		{
			this.size = size;
			this.syncThread = syncThread;
			this.logger = logger;
			timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
		}


		public void Execute(IServerTaskExecutionContext context)
		{
			CheckThread();

			logger.Log(LogLevel.Debug, BufferInitializedID, "Event buffer initialized in {Thread} thread", syncThread);

			this.context = context;
		}

		public void PerformTerminate()
		{
			CheckThread();

			isPerformTerminate = true;

			ExecuteAllEvents();

			onCompleted?.Invoke();
		}

		public IServerTaskObserver GetObserver()
		{
			CheckThread();

			return new Observer(this);
		}

		public void Dispatch(Action eventDelegate)
		{
			CheckThread();

			if (context is null)
				throw new InvalidOperationException("Buffer is not initialized, dispatch as server task to fix it");

			events.Add(eventDelegate);
			timer.Change((int)size.TotalMilliseconds, -1);
		}

		public void Dispose()
		{
			CheckThread();

			timer.Dispose();
		}

		private void TimerCallback(object? _)
		{
			if (context is null)
				return;

			context.PostAction(() =>
			{
				CheckThread();

				if (isPerformTerminate)
					return;

				ExecuteAllEvents();
			});
		}

		private void ExecuteAllEvents()
		{
			foreach (var eventDelegate in events)
			{
				try
				{
					eventDelegate();
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, ExceptionInEventID, ex, "Some event finished with exception in {Thread} thread", syncThread);
				}
			}

			events.Clear();
		}

		private void CheckThread([CallerMemberName] string nameOfCaller = "")
		{
			if (Environment.CurrentManagedThreadId != syncThread)
				throw new ThreadAccessException(nameof(EventBuffer), 0, nameOfCaller);
		}


		private sealed class Observer : IServerTaskObserver
		{
			private readonly EventBuffer owner;


			public Observer(EventBuffer owner)
			{
				this.owner = owner;
			}


			public void OnCompleted(Action continuation)
			{
				owner.onCompleted = continuation;
			}
		}
	}
}
