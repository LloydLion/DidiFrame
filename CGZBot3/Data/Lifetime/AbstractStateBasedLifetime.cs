using CGZBot3.Utils;
using CGZBot3.Utils.StateMachine;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Data.Lifetime
{
	public class AbstractStateBasedLifetime<TState, TBase> : ILifetime<TBase>
		where TState : struct
		where TBase : class, IStateBasedLifetimeBase<TState>
	{
		private readonly TBase baseObj;
		private ILifetimeStateUpdater<TBase>? updater;
		private readonly IStateMachineBuilder<TState> smBuilder;
		private IStateMachine<TState>? machine;
		private bool hasBuilt = false;
		private readonly Dictionary<TState, List<Action>> startupHandlers = new();


		public AbstractStateBasedLifetime(IServiceProvider services, TBase baseObj)
		{
			this.baseObj = baseObj;
			smBuilder = services.GetRequiredService<IStateMachineBuilderFactory<TState>>().Create(GetType().FullName ?? throw new ImpossibleVariantException());
		}


		public ObjectHolder<TBase> GetBase()
		{
			var startState = baseObj.State;

			return new ObjectHolder<TBase>(baseObj, (holder) =>
			{
				Exception? ex = null;

				if (!startState.Equals(baseObj.State))
				{
					ex = new InvalidOperationException("Enable to change state in base object of lifetime. State reverted and object saved");
					baseObj.State = startState;
				}

				GetUpdater().Update(this);

				if (ex is not null) throw ex;
			});
		}

		public TBase GetBaseClone()
		{
			return (TBase)baseObj.Clone();
		}

		public void Run(ILifetimeStateUpdater<TBase> updater)
		{
			this.updater = updater;
			machine = smBuilder.Build();
			hasBuilt = true;
			machine.Start(baseObj.State);

			foreach (var item in startupHandlers)
				foreach (var handler in item.Value) handler();
		}

		protected ILifetimeStateUpdater<TBase> GetUpdater()
		{
			if (!hasBuilt)
				throw new InvalidOperationException("Enable to get updater before starting");
			return updater ?? throw new ImpossibleVariantException();
		}

		protected IStateMachine<TState> GetStateMachine()
		{
			if (!hasBuilt)
				throw new InvalidOperationException("Enable to get state machine before starting");
			return machine ?? throw new ImpossibleVariantException();
		}

		private void ThrowIfBuilt()
		{
			if (hasBuilt)
				throw new InvalidOperationException("State machine done, please don't invoke any constructing methods");
		}

		private static Func<CancellationToken, Task> AwaitCycleTask(Func<CancellationToken, Task<bool>> taskCreator)
		{
			return async (token) =>
			{
				while (token.IsCancellationRequested == false && await taskCreator.Invoke(token) == false)
					await Task.Delay(100, token);
			};
		}

		//Constructing methods
		protected void AddTransit(IStateTransitWorker<TState> worker)
		{
			ThrowIfBuilt();
			smBuilder.AddStateTransitWorker(worker);
		}

		protected void AddTransit(TState from, TState? to, int delay) =>
			AddTransit(new TaskTransitWorker<TState>(from, to, (token) => Task.Delay(delay, token)));

		protected void AddTransit(TState from, TState? to, Func<CancellationToken, Task<bool>> taskCreator) =>
			AddTransit(new TaskTransitWorker<TState>(from, to, AwaitCycleTask(taskCreator)));

		protected void AddTransit(TState from, TState? to, Func<bool> predicate) =>
			AddTransit(new PredicateTransitWorker<TState>(from, to, predicate));

		protected void AddStartup(TState state, Action handler)
		{
			ThrowIfBuilt();
			if(startupHandlers.ContainsKey(state) == false) startupHandlers.Add(state, new());
			startupHandlers[state].Add(handler);
		}

		protected void AddHandler(TState state, Action<TState> handler)
		{
			ThrowIfBuilt();
			smBuilder.AddStateChangedHandler((sm, old) =>
				{ if (sm.CurrentState.Equals(state)) handler.Invoke(old); });
		}
	}
}
