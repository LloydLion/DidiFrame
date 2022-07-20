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
		private readonly IServersStatesRepository<ICollection<TBase>> states;
		private readonly ILifetimeFactory<TLifetime, TBase> lifetimeFactory;
		private readonly ConcurrentDictionary<IServer, ServerLifetimeState> lifetimes;


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.LifetimeRegistry`2
		/// </summary>
		/// <param name="repository">Repository for push lifetimes</param>
		/// <param name="baseRepository">Repository to provide states</param>
		public LifetimeRegistry(IServersStatesRepository<ICollection<TBase>> states, ILifetimeFactory<TLifetime, TBase> lifetimeFactory)
		{
			this.states = states;
			this.lifetimeFactory = lifetimeFactory;
		}


		/// <inheritdoc/>
		public void RestoreLifetimes(IServer server)
		{
			var state = states.GetState(server);
			using var collection = state.Open();
			foreach (var item in collection.Object)
				CreateLifetimeInstance(item, true);
		}

		public TLifetime RegistryLifetime(TBase baseObject) => CreateLifetimeInstance(baseObject, false);

		public TLifetime GetLifetime(IServer server, Guid baseGuid)
		{
			return lifetimes.GetOrAdd(server, _ => new()).Lifetimes[baseGuid];	
		}

		public IReadOnlyCollection<TLifetime> GetAllLifetimes(IServer server)
		{
			return lifetimes.GetOrAdd(server, _ => new()).Lifetimes.Values.ToArray();
		}

		private TLifetime CreateLifetimeInstance(TBase baseObject, bool isRestore)
		{
			var state = states.GetState(baseObject.Server);
			using var collection = state.Open();
			var lifetime = lifetimeFactory.Create();
			var sls = lifetimes.GetOrAdd(baseObject.Server, _ => new());
			collection.Object.Add(baseObject);
			sls.Lifetimes.TryAdd(baseObject.Guid, lifetime);

			var holder = new BaseObjectHolder(state, sls, baseObject.Guid);
			var context = new DefaultLifetimeContext<TBase>(!isRestore, holder, holder.FinalizeObject);

			lifetime.Run(baseObject, context);

			return lifetime;
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

			public void FinalizeObject(Exception? _1)
			{
				using var collection = state.Open();
				collection.Object.Remove(collection.Object.Single(s => s.Guid == baseId));
				sls.Lifetimes.TryRemove(baseId, out _);
			}
		}
	}
}
