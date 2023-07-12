using DidiFrame.Exceptions;
using DidiFrame.Threading;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp.Server
{
	internal class ServerTaskCollection
	{
		private readonly HashSet<IServerTask> tasks = new();
		private readonly int targetThreadId;
		private readonly DSharpServer owner;
		private readonly ILogger<ServerTaskCollection> logger;
		private TerminationContext? termination;


		public ServerTaskCollection(DSharpServer owner, int targetThreadId)
		{
			this.targetThreadId = targetThreadId;
			this.owner = owner;
			logger = owner.BaseClient.LoggerFactory.CreateLogger<ServerTaskCollection>();
		}


		public void DispatchTask(IServerTask task)
		{
			CheckThread();

			if (termination is not null)
				throw new InvalidOperationException($"{owner} is in PerformTermination status or later status (see Status property), enable to create new server task");


			try
			{
				tasks.Add(task);
				task.GetObserver().OnCompleted(() => OnTaskCompleted(task));

				var executionContext = new Context(owner.WorkQueue);

				owner.WorkQueue.Dispatch(() => task.Execute(executionContext));
			}
			catch (Exception)
			{
				tasks.Remove(task);
				throw;
			}
		}

		public Task FinalizeAsync()
		{
			CheckThread();

			foreach (var task in tasks)
			{
				try
				{
					task.PerformTerminate();
				}
				catch (Exception ex)
				{
					//TODO: log all action and add EventIds
					logger.Log(LogLevel.Error, ex,
						"[ServerTaskCollection of {Owner}]: Some task thrown an exception when system tries to perform terminate it. [Task object: {Task} of type {TaskType}]",
						owner.ToString(), task.ToString(), task.GetType().FullName);
				}
			}

			var tcs = new TaskCompletionSource();
			termination = new TerminationContext(tcs);

			// If we have no tasks OnTaskCompleted will not be called and will not set result to tcs,
			// in this case we should finish instantly (return aka Task.CompletedTask), but termination context should be set
			if (tasks.Count == 0)
				tcs.SetResult();

			return tcs.Task;
		}

		private void OnTaskCompleted(IServerTask task)
		{
			CheckThread();

			tasks.Remove(task);
			task.Dispose();

			if (termination is not null && tasks.Count == 0)
				termination.Value.Task.SetResult();
		}

		private void CheckThread([CallerMemberName] string nameOfCaller = "")
		{
			if (Environment.CurrentManagedThreadId != targetThreadId)
				throw new ThreadAccessException(nameof(DSharpServer), owner.Id, nameOfCaller);
		}


		private record struct TerminationContext(TaskCompletionSource Task);

		private sealed class Context : IServerTaskExecutionContext
		{
			private readonly IManagedThreadExecutionQueue serverWorkQueue;


			public Context(IManagedThreadExecutionQueue serverWorkQueue)
			{
				this.serverWorkQueue = serverWorkQueue;
			}


			public void PostAction(Action action)
			{
				serverWorkQueue.Dispatch(new ManagedThreadTask(action));
			}
		}
	}
}
