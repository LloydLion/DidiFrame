using DidiFrame.Utils;
using System.Collections.Concurrent;

namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Simple implementation of DidiFrame.Lifetimes.ILifetimesRegistry`2
	/// </summary>
	/// <typeparam name="TLifetime">Type of target lifetime</typeparam>
	/// <typeparam name="TBase">Type of base object of that lifetime</typeparam>
	public class LifetimeRegistry<TLifetime, TBase> : ILifetimesRegistry<TLifetime, TBase> where TLifetime : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		private static readonly EventId LifetimeCrachedID = new(11, "LifetimeCrashed");
		private static readonly EventId LifetimeRegistryFailID = new(23, "LifetimeRegistryFail");
		private static readonly EventId LifetimeRestoreFailID = new(24, "LifetimeRestoreFail");


		private readonly IServersStatesRepository<ICollection<TBase>> states;
		private readonly ILifetimeFactory<TLifetime, TBase> lifetimeFactory;
		private readonly ILogger<LifetimeRegistry<TLifetime, TBase>> logger;
		private readonly ConcurrentDictionary<IServer, ServerLifetimeState> lifetimes = new();


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.LifetimeRegistry`2
		/// </summary>
		/// <param name="states">Repository with states</param>
		/// <param name="lifetimeFactory">Factory for target lifetime</param>
		/// <param name="logger">Logger for registry</param>
		public LifetimeRegistry(IServersStatesRepository<ICollection<TBase>> states, ILifetimeFactory<TLifetime, TBase> lifetimeFactory, ILogger<LifetimeRegistry<TLifetime, TBase>> logger)
		{
			this.states = states;
			this.lifetimeFactory = lifetimeFactory;
			this.logger = logger;
		}


		/// <inheritdoc/>
		public void RestoreLifetimes(IServer server)
		{
			var state = states.GetState(server);
			var failed = new List<TBase>();

			using var collection = state.Open();

			foreach (var item in collection.Object)
			{
				var lifetime = lifetimeFactory.Create();
				var sls = lifetimes.GetOrAdd(item.Server, _ => new());
				sls.Lifetimes.TryAdd(item.Guid, lifetime);

				var holder = new BaseObjectHolder(state, sls, item.Guid);
				var context = new DefaultLifetimeContext<TBase>(lifetime, isNewlyCreated: false, holder, holder.FinalizeObject,
					LogError(lifetime.GetType(), item.Guid, item.Server));

				try
				{
					lifetime.Run(item, context);
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, LifetimeRestoreFailID, ex, "Failed to restore lifetime ({LifetimeType}) {LifetimeId} on {ServerId}", lifetime.GetType(), item.Guid, item.Server.Id);
					failed.Add(item);
					sls.Lifetimes.TryRemove(item.Guid, out _);
				}
			}

			foreach (var item in failed) collection.Object.Remove(item);
		}

		/// <inheritdoc/>
		public TLifetime RegistryLifetime(TBase baseObject)
		{
			var state = states.GetState(baseObject.Server);

			using var collection = state.Open();

			var lifetime = lifetimeFactory.Create();
			var sls = lifetimes.GetOrAdd(baseObject.Server, _ => new());
			collection.Object.Add(baseObject);
			sls.Lifetimes.TryAdd(baseObject.Guid, lifetime);

			var holder = new BaseObjectHolder(state, sls, baseObject.Guid);
			var context = new DefaultLifetimeContext<TBase>(lifetime, isNewlyCreated: true, holder, holder.FinalizeObject,
				LogError(lifetime.GetType(), baseObject.Guid, baseObject.Server));

			try
			{
				lifetime.Run(baseObject, context);
				return lifetime;
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, LifetimeRegistryFailID, ex, "Failed to restore lifetime ({LifetimeType}) {LifetimeId} on {ServerId}", lifetime.GetType(), baseObject.Guid, baseObject.Server.Id);
				collection.Object.Remove(baseObject);
				sls.Lifetimes.TryRemove(baseObject.Guid, out _);
				throw;
			}
		}

		/// <inheritdoc/>
		public TLifetime GetLifetime(IServer server, Guid baseGuid)
		{
			return lifetimes.GetOrAdd(server, _ => new()).Lifetimes[baseGuid];	
		}

		/// <inheritdoc/>
		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server)
		{
			return lifetimes.GetOrAdd(server, _ => new()).Lifetimes.Values.ToArray();
		}

		private Action<Exception, bool> LogError(Type typeOfLifetime, Guid id, IServer server)
		{
			return (exception, isInvalidModel) =>
				logger.Log(LogLevel.Error, LifetimeCrachedID, exception, "({LifetimeType}) Lifetime {LifetimeId} has crashed on {ServerId}! Model was {InvalidStatus}",
					typeOfLifetime, id, server.Id, isInvalidModel ? "Invalid" : "Valid");
		}


		private struct ServerLifetimeState
		{
			public ServerLifetimeState()
			{
				Lifetimes = new();
			}


			public ConcurrentDictionary<Guid, TLifetime> Lifetimes { get; }
		}

		private class BaseObjectHolder : IObjectController<TBase>
		{
			private readonly IObjectController<ICollection<TBase>> state;
			private readonly ServerLifetimeState sls;
			private readonly Guid baseId;


			public BaseObjectHolder(IObjectController<ICollection<TBase>> state, ServerLifetimeState sls, Guid baseId)
			{
				this.state = state;
				this.sls = sls;
				this.baseId = baseId;
			}


			public ObjectHolder<TBase> Open()
			{
				var holder = state.Open();

				try
				{
					var model = holder.Object.Single(s => s.Guid == baseId);
					return new ObjectHolder<TBase>(model, _ => { holder.Dispose(); });
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
				collection.Object.Remove(collection.Object.Single(s => s.Guid == baseId));
				sls.Lifetimes.TryRemove(baseId, out _);
			}
		}
	}
}
