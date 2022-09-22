using DidiFrame.Utils;
using DidiFrame.Utils.Collections;
using System.Collections.Concurrent;
using static DidiFrame.Lifetimes.LifetimeRegistryStatic;

namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Simple implementation of DidiFrame.Lifetimes.ILifetimesRegistry`2
	/// </summary>
	/// <typeparam name="TLifetime">Type of target lifetime</typeparam>
	/// <typeparam name="TBase">Type of base object of that lifetime</typeparam>
	public sealed class LifetimeRegistry<TLifetime, TBase> : ILifetimesRegistry<TLifetime, TBase>, IDisposable where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		private readonly IServersStatesRepository<ICollection<TBase>> states;
		private readonly ILifetimeInstanceCreator<TLifetime, TBase> lifetimeInstanceCreator;
		private readonly ILogger<LifetimeRegistry<TLifetime, TBase>> logger;
		private readonly IServersNotify serverNotify;
		private readonly ConcurrentDictionary<IServer, ServerLifetimeState> lifetimes = new();


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.LifetimeRegistry`2
		/// </summary>
		/// <param name="states">Repository with states</param>
		/// <param name="lifetimeInstanceCreator">Factory for target lifetime</param>
		/// <param name="logger">Logger for registry</param>
		/// <param name="serverNotify">Server delete notifier to delete per server resources</param>
		public LifetimeRegistry(IServersStatesRepository<ICollection<TBase>> states,
			ILifetimeInstanceCreator<TLifetime, TBase> lifetimeInstanceCreator,
			ILogger<LifetimeRegistry<TLifetime, TBase>> logger,
			IServersNotify serverNotify)
		{
			this.states = states;
			this.lifetimeInstanceCreator = lifetimeInstanceCreator;
			this.logger = logger;
			this.serverNotify = serverNotify;
			serverNotify.ServerRemoved += OnServerRemoved;
		}


		/// <inheritdoc/>
		public void RestoreLifetimes(IServer server)
		{
			var state = states.GetState(server);
			var failed = new List<TBase>();

			using var collection = state.Open();

			foreach (var item in collection.Object)
			{
				var sls = lifetimes.GetOrAdd(item.Server, _ => new(logger, server));

				var lifetime = lifetimeInstanceCreator.Create();

				var sych = sls.UpdateDispatcher.GetSynchronizationContext();
				var holder = new BaseObjectController(state, item.Id, sych);
				var context = new DefaultLifetimeContext<TBase>(isNewlyCreated: false, holder, sych);

				lock (sls.Lifetimes)
				{
					sls.Lifetimes.Add(item.Id, new(lifetime, context, holder));

					try
					{
						lifetime.Run(item, context);
					}
					catch (Exception ex)
					{
						logger.Log(LogLevel.Error, LifetimeRestoreFailID, ex, "Failed to restore lifetime ({LifetimeType}) {LifetimeId} in {ServerId}", lifetime.GetType(), item.Id, item.Server.Id);
						failed.Add(item);
						sls.Lifetimes.Remove(item.Id);
						lifetime.Dispose();
					}
				}
			}

			foreach (var item in failed) collection.Object.Remove(item);
		}

		/// <inheritdoc/>
		public TLifetime RegistryLifetime(TBase baseObject)
		{
			var state = states.GetState(baseObject.Server);

			using var collection = state.Open();

			var sls = lifetimes.GetOrAdd(baseObject.Server, _ => new(logger, baseObject.Server));

			lock (sls.Lifetimes)
			{
				if (baseObject.Server.IsClosed)
					throw new ArgumentException("Target server was closed in lifetime creationg process", nameof(baseObject));

				var lifetime = lifetimeInstanceCreator.Create();
				collection.Object.Add(baseObject);

				var sych = sls.UpdateDispatcher.GetSynchronizationContext();
				var holder = new BaseObjectController(state, baseObject.Id, sych);
				var context = new DefaultLifetimeContext<TBase>(isNewlyCreated: true, holder, sych);

				sls.Lifetimes.Add(baseObject.Id, new(lifetime, context, holder));

				try
				{
					lifetime.Run(baseObject, context);
					return lifetime;
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, LifetimeRegistryFailID, ex, "Failed to registry lifetime ({LifetimeType}) {LifetimeId} in {ServerId}", lifetime.GetType(), baseObject.Id, baseObject.Server.Id);
					collection.Object.Remove(baseObject);
					sls.Lifetimes.Remove(baseObject.Id);
					lifetime.Dispose();
					throw;
				}
			}
		}

		/// <inheritdoc/>
		public TLifetime GetLifetime(IServer server, Guid baseGuid)
		{
			var serverLfs = lifetimes.GetOrAdd(server, _ => new(logger, server)).Lifetimes;
			lock (serverLfs)
			{
				return serverLfs[baseGuid].Lifetime;
			}
		}

		/// <inheritdoc/>
		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server)
		{
			var serverLfs = lifetimes.GetOrAdd(server, _ => new(logger, server)).Lifetimes;
			lock (serverLfs)
			{
				return SelectCollection<TLifetime>.Create(serverLfs.Values, s => s.Lifetime).ToArray();
			}
		}

		private void OnServerRemoved(IServer server)
		{
			lifetimes.TryRemove(server, out var sls);

			if (sls is not null)
					sls.UpdateDispatcher.Dispose();
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			serverNotify.ServerRemoved -= OnServerRemoved;
		}


		private sealed class ServerLifetimeState : IDisposable
		{
			public ServerLifetimeState(ILogger logger, IServer server)
			{
				Lifetimes = new();
				UpdateDispatcher = new(Lifetimes, logger, server);
			}


			public Dictionary<Guid, LifetimeEntry> Lifetimes { get; }

			public LifetimeUpdateDispatcher UpdateDispatcher { get; }


			public void Dispose() => UpdateDispatcher.Dispose();
		}

		private sealed class LifetimeEntry
		{
			public LifetimeEntry(TLifetime lifetime, DefaultLifetimeContext<TBase> context, BaseObjectController controller)
			{
				if (Equals(controller, context.AccessBase()) == false)
					throw new ArgumentException("Invalid transmited controller, it must be equal to controller in context", nameof(controller));

				Lifetime = lifetime;
				Context = context;
				Controller = controller;
			}


			public TLifetime Lifetime { get; }

			public DefaultLifetimeContext<TBase> Context { get; }

			public BaseObjectController Controller { get; }
		}

		private sealed class BaseObjectController : IObjectController<TBase>
		{
			private readonly IObjectController<ICollection<TBase>> state;
			private readonly Guid baseId;
			private readonly LifetimeSynchronizationContext context;


			public BaseObjectController(IObjectController<ICollection<TBase>> state, Guid baseId, LifetimeSynchronizationContext context)
			{
				this.state = state;
				this.baseId = baseId;
				this.context = context;
			}


			public ObjectHolder<TBase> Open()
			{
				var buffer = new ObjectHolder<TBase>[1];
				context.Send(SendDelegate, buffer);
				return buffer[0];
			}

			private void SendDelegate(object? rawOutput)
			{
				var output = (ObjectHolder<TBase>[])(rawOutput ?? throw new NullReferenceException());
				var holder = state.Open();

				try
				{
					var model = holder.Object.Single(s => s.Id == baseId);
					output[0] = new ObjectHolder<TBase>(model, _ => { holder.Dispose(); });
				}
				catch (Exception)
				{
					holder.Dispose();
					throw;
				}
			}

			public void FinalizeObject()
			{
				using var collection = state.Open();
				collection.Object.Remove(collection.Object.Single(s => s.Id == baseId));
			}
		}

		private sealed class LifetimeUpdateDispatcher : IDisposable
		{
			private readonly Thread updateThread;
			private readonly CancellationTokenSource cts = new();
			private readonly Dictionary<Guid, LifetimeEntry> lifetimes;
			private readonly ConcurrentQueue<DispatcherTask> tasks = new();
			private readonly AutoResetEvent tasksEnqueueEvent = new(initialState: false);
			private readonly LTSychContext synchronizationContext;
			private readonly ILogger logger;
			private readonly IServer server;


			public LifetimeUpdateDispatcher(Dictionary<Guid, LifetimeEntry> lifetimes, ILogger logger, IServer server)
			{
				updateThread = CreateUpdateThread(cts.Token);
				this.lifetimes = lifetimes;
				this.logger = logger;
				this.server = server;
				synchronizationContext = new(tasks, tasksEnqueueEvent, updateThread.ManagedThreadId);
			}


			private Thread CreateUpdateThread(CancellationToken token)
			{
				var thread = new Thread(() =>
				{
					var deletedLifetimes = new List<Guid>();

					bool firstIteration = true;

					while (token.IsCancellationRequested == false)
					{
						if (firstIteration || tasksEnqueueEvent.WaitOne(LifetimesUpdateTimeoutInMilliseconds) == false)
						{
							firstIteration = false;

							lock (lifetimes)
							{
								foreach (var item in lifetimes)
								{
									var entry = item.Value;

									if (entry.Context.IsFinalized)
										deletedLifetimes.Add(item.Key);

									try
									{
										entry.Lifetime.Update();
									}
									catch (Exception ex)
									{
										var typeOfLifetime = entry.Lifetime.GetType();

										logger.Log(LogLevel.Error, LifetimeCrachedID, ex, "({LifetimeType}) Lifetime {LifetimeId} has crashed in {ServerId}!", typeOfLifetime, item.Key, server.Id);

										if (entry.Context.IsFinalized == false)
											entry.Context.FinalizeLifetime();
									}

									if (entry.Context.IsFinalized)
										deletedLifetimes.Add(item.Key);
								}

								foreach (var lifetime in deletedLifetimes)
								{
									if (lifetimes.Remove(lifetime, out var entry))
									{
										//Lifetime disposing, order is important
										entry.Lifetime.Dispose();
										entry.Controller.FinalizeObject();
									}
								}

								deletedLifetimes.Clear();
							}
						}
						else
						{
							while (tasks.TryDequeue(out var task))
							{
								try
								{
									task.DelegateToExecute(task.State);
									task.Callback?.Invoke();
								}
								catch (Exception ex)
								{
									task.ExceptionHandler?.Invoke(ex);
								}
							}
						}
					}
				});

				thread.Start();
				return thread;
			}

			public LifetimeSynchronizationContext GetSynchronizationContext() => synchronizationContext;

			public void Dispose()
			{
				cts.Cancel();
				updateThread.Join();

				lock (lifetimes)
				{
					foreach (var lifetime in lifetimes.Values.Select(s => s.Lifetime))
					{
						lifetime.Destroy();
						lifetime.Dispose();
					}
				}
			}


			private sealed class LTSychContext : LifetimeSynchronizationContext
			{
				private readonly ConcurrentQueue<DispatcherTask> sharedTasks;
				private readonly AutoResetEvent enqueueEvent;
				private readonly int targetThreadId;


				public LTSychContext(ConcurrentQueue<DispatcherTask> sharedTasks, AutoResetEvent enqueueEvent, int targetThreadId)
				{
					this.sharedTasks = sharedTasks;
					this.enqueueEvent = enqueueEvent;
					this.targetThreadId = targetThreadId;
				}


				public override void Post(SendOrPostCallback d, object? state)
				{
					var task = new DispatcherTask(d, state, null, null);
					sharedTasks.Enqueue(task);
				}

				public override void Send(SendOrPostCallback d, object? state)
				{
					if (Environment.CurrentManagedThreadId == targetThreadId)
						d(state);
					else
					{
						var eventTrigger = new AutoResetEvent(initialState: false);
						var exceptionBuffer = new Exception[1];
						var task = new DispatcherTask(d, state, () => eventTrigger.Set(), ex => exceptionBuffer[0] = ex);
						sharedTasks.Enqueue(task);
						enqueueEvent.Set();
						eventTrigger.WaitOne();
						if (exceptionBuffer[0] is not null)
							throw new AggregateException(exceptionBuffer[0]);
					}
				}
			}


			private sealed record DispatcherTask(SendOrPostCallback DelegateToExecute, object? State, Action? Callback, Action<Exception>? ExceptionHandler);
		}
	}

	internal static class LifetimeRegistryStatic
	{
		public static readonly EventId LifetimeCrachedID = new(11, "LifetimeCrashed");
		public static readonly EventId LifetimeRegistryFailID = new(23, "LifetimeRegistryFail");
		public static readonly EventId LifetimeRestoreFailID = new(24, "LifetimeRestoreFail");
		public const int LifetimesUpdateTimeoutInMilliseconds = 1000;
	}
}
