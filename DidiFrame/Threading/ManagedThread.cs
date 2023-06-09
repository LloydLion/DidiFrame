//#define LogFullTraceInfo

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using ThreadState = System.Threading.ThreadState;

namespace DidiFrame.Threading
{
	internal class ManagedThread : IManagedThread
	{
		private static readonly EventId InternalTaskErrorID = new(91, "InternalTaskError");
		private static readonly EventId ThreadStartedID = new(11, "ThreadStarted");
		private static readonly EventId ThreadStoppedID = new(12, "ThreadStopped");
		private static readonly EventId NewExecutionQueueCreatedID = new(13, "NewExecutionQueueCreated");
		private static readonly EventId ExecutionQueueChangedID = new(14, "ExecutionQueueChanged");
		private static readonly EventId TaskDispatchedID = new(21, "TaskDispatched");
		private static readonly EventId TaskExecutedID = new(22, "TaskExecuted");


		private readonly Thread internalThread;
		private readonly ILogger logger;
		private bool prependStop = false;
		private TaskQueue? currentQueue;
		private InternalThreadTask? currentExecutingTask;


		public ManagedThread(ILogger logger)
		{
			internalThread = new Thread(ThreadWorker);
			this.logger = logger;
			internalThread.Name = "Thread for " + ToString();
		}


		public bool IsInside => Environment.CurrentManagedThreadId == internalThread.ManagedThreadId;

		private TaskQueue CurrentQueue => currentQueue ?? throw new InvalidOperationException("These is no any queue");


		public void Begin(IManagedThreadExecutionQueue queue)
		{
			if (internalThread.ThreadState != ThreadState.Unstarted)
				throw new InvalidOperationException("Thread was stared");

			SetExecutionQueueInternal(queue);
			internalThread.Start();

			logger.Log(LogLevel.Debug, ThreadStartedID, "[{This}]: Stated new managed thread with id {ID}", ToString(), internalThread.ManagedThreadId);
		}

		public IManagedThreadExecutionQueue CreateNewExecutionQueue(string queueName)
		{
			logger.Log(LogLevel.Debug, NewExecutionQueueCreatedID, "[{This}]: New execution queue created with name {Name}", ToString(), queueName);
			return new TaskQueue(this, queueName);
		}

		public IManagedThreadExecutionQueue GetActiveQueue()
		{
			return CurrentQueue;
		}

		public void SetExecutionQueue(IManagedThreadExecutionQueue queue)
		{
			CheckAccess();
			SetExecutionQueueInternal(queue);
		}

		public void Stop()
		{
			CheckAccess();

			if (prependStop)
				throw new InvalidOperationException("Enable to stop thread twice");

			prependStop = true;

			logger.Log(LogLevel.Debug, ThreadStoppedID, "[{This}]: Thread stopped, bye", ToString());
		}

		public override string? ToString()
		{
			return $"ManagedThread #{internalThread.ManagedThreadId}";
		}

		private void CheckAccess()
		{
			if (IsInside == false)
				throw new InvalidOperationException($"Call from invalid thread, you can operate with it only from {internalThread} thread");
		}

		private void SetExecutionQueueInternal(IManagedThreadExecutionQueue queue)
		{
			var tq = (TaskQueue)queue;
			tq.CheckAccess(this);
			currentQueue?.Dispose();
			currentQueue = tq;

			logger.Log(LogLevel.Debug, ExecutionQueueChangedID, "[{This}]: Execution queue changed to \"{Name}\"", ToString(), tq.Name);
		}

		//If you change task execution flow DON'T forget change InternalThreadTask.DebugState.FrameToSlice constant
		private void ThreadWorker()
		{
			while (prependStop == false)
			{
				CurrentQueue.OnNewTask.WaitOne();

				while (CurrentQueue.Queue.TryDequeue(out var task))
				{
					currentExecutingTask = task;
					SynchronizationContext.SetSynchronizationContext(CurrentQueue.Context);

					try
					{
						task.Task.Invoke();
						logger.Log(LogLevel.Trace, TaskExecutedID, "[{This}]: Task executed. Task delegate: {Task}", ToString(), task.Task);
					}
					catch (Exception ex)
					{
#if DEBUG || LogFullTraceInfo
						logger.Log(LogLevel.Error, InternalTaskErrorID, ex, "[{This}]: Error in executing task. Task delegate: {Task}. (ONLY IN DEBUG) {Debug}", ToString(), task.Task, task.Debug);
#else
						logger.Log(LogLevel.Error, InternalTaskErrorID, ex, "[{This}]: Error in executing task. Task delegate: {Task}", ToString(), task.Task);
#endif
					}

					currentExecutingTask = null;
				}
			}
		}


		private sealed class TaskQueue : IManagedThreadExecutionQueue, IDisposable
		{
			private readonly ManagedThread owner;
			private readonly SynchronizationContext context;
			private bool disposed;


			public TaskQueue(ManagedThread owner, string name)
			{
				this.owner = owner;
				Name = name;
				context = new ManagedQueueBasedSynchronizationContext(this);
			}


			public ConcurrentQueue<InternalThreadTask> Queue { get; } = new();

			public AutoResetEvent OnNewTask { get; } = new(false);

			public string Name { get; }

			public bool IsDisposed => disposed;

			public IManagedThread Thread => owner;

			public SynchronizationContext Context => context;


			public void Dispatch(ManagedThreadTask task)
			{
				CheckDisposed();

				owner.logger.Log(LogLevel.Debug, TaskDispatchedID, "[{This}]: New task dispatched. Task delegate: {Task}", ToString(), task);

#if DEBUG || LogFullTraceInfo
				InternalThreadTask? ownerTask = null;
				if (Environment.CurrentManagedThreadId == owner.internalThread.ManagedThreadId)
					ownerTask = owner.currentExecutingTask;
				var callstack = new StackTrace();
				Queue.Enqueue(new InternalThreadTask(task, new(callstack, ownerTask?.Debug)));
#else
				Queue.Enqueue(new InternalThreadTask(task));
#endif
				OnNewTask.Set();
			}

			public void CheckAccess(ManagedThread thread)
			{
				CheckDisposed();

				if (thread != owner)
					throw new InvalidOperationException("Owner mismatch! Enable to operate with queue from another thread");
			}

			public void Dispose()
			{
				CheckDisposed();
				disposed = true;
				OnNewTask.Dispose();
			}

			public override string? ToString()
			{
				return $"ManagedThread.TaskQueue [{Name}] for {owner}";
			}

			private void CheckDisposed()
			{
				if (disposed)
					throw new ObjectDisposedException("This execution queue is disposed. NOTE: execution queue can be used only one time");
			}
		}

		private sealed class InternalThreadTask
		{
#if DEBUG || LogFullTraceInfo
			public InternalThreadTask(ManagedThreadTask task, DebugState debugState)
			{
				Task = task;
				Debug = debugState;
			}
#else
			public InternalThreadTask(ManagedThreadTask task)
			{
				Task = task;
			}
#endif
		

			public ManagedThreadTask Task { get; }

#if DEBUG || LogFullTraceInfo
			public DebugState Debug { get; }


			public record DebugState(StackTrace Callstack, DebugState? Creator)
			{
				//Call stack "service" frame count that should be sliced in logs
				private const int FrameToSlice = 1;


				public override string ToString()
				{
					var result = new List<string>();

					DebugState? state = this;

					while (state is not null)
					{
						var callstackText = new StringBuilder();

						var isDegenerate = state.Creator is not null;

						var frames = isDegenerate ? state.Callstack.GetFrames().Skip(FrameToSlice) : state.Callstack.GetFrames();

						foreach (var frame in frames)
						{
							var method = frame.GetMethod();
							if (method is not null)
							{
								var methodVisual = $"{method.DeclaringType?.FullName}.{method.Name}[{string.Join(", ", method.GetGenericArguments().AsEnumerable())}]({string.Join(", ", method.GetParameters().AsEnumerable())})";
								var filePosition = frame.HasSource() ? $" in {frame.GetFileName()} at {frame.GetFileLineNumber}:{frame.GetFileColumnNumber()}" : null;
								callstackText.AppendLine(methodVisual + filePosition);
							}
							else
							{
								callstackText.AppendLine($"Native frame, native offset: {frame.GetNativeOffset():X8}"); //X8 - as 8 hex digits
							}
						}

						result.Add(callstackText.ToString());

						state = state.Creator;
					}

					result.Reverse();
					return string.Join($"\n{new string('-', 50)}\n", result);
				}
			}
#endif
		}
	}
}
