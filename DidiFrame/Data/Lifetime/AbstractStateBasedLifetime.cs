using DidiFrame.Utils;
using DidiFrame.Utils.StateMachine;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Data.Lifetime
{
	/// <summary>
	/// Base class of statemachine-based lifetime
	/// </summary>
	/// <typeparam name="TState">Internal statemachine state type</typeparam>
	/// <typeparam name="TBase">Type of base object of that lifetime </typeparam>
	public abstract class AbstractStateBasedLifetime<TState, TBase> : ILifetime<TBase>
		where TState : struct
		where TBase : class, IStateBasedLifetimeBase<TState>
	{
		private readonly TBase baseObj;
		private ILifetimeStateUpdater<TBase>? updater;
		private readonly StateMachineBuilder<TState> smBuilder;
		private IStateMachine<TState>? machine;
		private bool hasBuilt = false;
		private readonly Dictionary<TState, List<Action>> startupHandlers = new();
		private readonly List<Action<TState>> stateHandlers = new();
		private static readonly ThreadLocker<AbstractStateBasedLifetime<TState, TBase>> baseLocker = new();


		/// <summary>
		/// Creates new instance of DidiFrame.Data.Lifetime.AbstractStateBasedLifetime`2
		/// </summary>
		/// <param name="services">Serivces that will be available in the future</param>
		/// <param name="baseObj">Base object of that lifetime</param>
		public AbstractStateBasedLifetime(IServiceProvider services, TBase baseObj)
		{
			this.baseObj = baseObj;

			var tname = GetType().FullName ?? throw new ImpossibleVariantException();
			smBuilder = new StateMachineBuilder<TState>(services.GetRequiredService<ILoggerFactory>().CreateLogger(tname));
		}


		/// <summary>
		/// Provides change-safe and thread-safe access to base object, automaticly notify state updater and freeze state machine util return disposed
		/// </summary>
		/// <param name="smFreeze">Statemachine freeze information, it will be automaticly disposed</param>
		/// <returns>DidiFrame.Utils.ObjectHolder`1 objects that must be disposed after wrtings</returns>
		protected ObjectHolder<TBase> GetBase(out FreezeModel<TState> smFreeze)
		{
			var smFreezeIn = GetStateMachine().Freeze();
			var lockFree = baseLocker.Lock(this);
			smFreeze = smFreezeIn;

			var startState = baseObj.State;

			return new ObjectHolder<TBase>(baseObj, (holder) =>
			{
				Exception? ex = null;

				if (!startState.Equals(baseObj.State))
				{
					ex = new InvalidOperationException("Enable to change state in base object of lifetime. State reverted and object saved");
					baseObj.State = startState;
				}

				lockFree.Dispose();
				smFreezeIn.Dispose();

				GetUpdater().Update(this);

				if (ex is not null) throw ex;
			});
		}

		/// <summary>
		/// Provides change-safe and thread-safe access to base object, automaticly notify state updater and freeze state machine util return disposed
		/// </summary>
		/// <returns>DidiFrame.Utils.ObjectHolder`1 objects that must be disposed after wrtings</returns>
		protected ObjectHolder<TBase> GetBase() => GetBase(out _);

		/// <inheritdoc/>
		public TBase GetBaseClone()
		{
			using (baseLocker.Lock(this))
			{
				return (TBase)baseObj.Clone();
			}
		}

		/// <inheritdoc/>
		public void Run(ILifetimeStateUpdater<TBase> updater)
		{
			this.updater = updater;
			machine = smBuilder.Build();
			machine.StateChanged += Machine_StateChanged;
			hasBuilt = true;

			OnRun(baseObj.State);

			foreach (var item in startupHandlers)
				foreach (var handler in item.Value) handler();

			machine.Start(baseObj.State);
			
		}

		private void Machine_StateChanged(IStateMachine<TState> stateMahcine, TState oldState)
		{
			var cs = stateMahcine.CurrentState;
			if (cs.HasValue == false)
			{
				OnDispose();
				GetUpdater().Finish(this);
			}
			else
			{
				baseObj.State = cs.Value;
				foreach (var handler in stateHandlers) handler(oldState);
				GetUpdater().Update(this);
			}
		}

		/// <summary>
		/// Gets a cached lifetime updater instance. No manual updates required
		/// </summary>
		/// <returns>The cached updater</returns>
		/// <exception cref="InvalidOperationException">If invoked before Run() call (example in ctor)</exception>
		protected ILifetimeStateUpdater<TBase> GetUpdater()
		{
			if (!hasBuilt)
				throw new InvalidOperationException("Enable to get updater before starting");
			return updater ?? throw new ImpossibleVariantException();
		}

		/// <summary>
		/// Gets base object directly.
		/// WARNING! Returned object is readonly!
		/// If changed changed will be not writen to state.
		/// This operation is thread-safe
		/// </summary>
		/// <returns>Base object itself</returns>
		protected TBase GetBaseDirect()
		{
			using (baseLocker.Lock(this))
			{
				return baseObj;
			}
		}

		/// <summary>
		/// Gets internal statemahcine
		/// </summary>
		/// <returns>Statemachine</returns>
		/// <exception cref="InvalidOperationException">If invoked before Run() call (example in ctor)</exception>
		protected IStateMachine<TState> GetStateMachine()
		{
			if (!hasBuilt)
				throw new InvalidOperationException("Enable to get state machine before starting");
			return machine ?? throw new ImpossibleVariantException();
		}

		/// <summary>
		/// Event handler. Calls on start. You mustn't call base.OnRun(TState)
		/// </summary>
		/// <param name="state">Initial statemachine state</param>
		protected virtual void OnRun(TState state)
		{

		}

		/// <summary>
		/// Event handler. Calls on lifetime's lifecycle end. You mustn't call base.OnDispose(TState)
		/// </summary>
		protected virtual void OnDispose()
		{

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
		/// <summary>
		/// Must be invoked only in ctor! Directly adds transit worker into building statemachine
		/// </summary>
		/// <param name="worker">Transit worker itself</param>
		/// <exception cref="InvalidOperationException">If invoked outside ctor (after Run() calling)</exception>
		protected void AddTransit(IStateTransitWorker<TState> worker)
		{
			ThrowIfBuilt();
			smBuilder.AddStateTransitWorker(worker);
		}

		/// <summary>
		/// Must be invoked only in ctor! Adds transit worker into building statemachine that will do transit after given timeout
		/// </summary>
		/// <param name="from">Start state</param>
		/// <param name="to">Target state</param>
		/// <param name="delay">Transit timeout</param>
		/// <exception cref="InvalidOperationException">If invoked outside ctor (after Run() calling)</exception>
		protected void AddTransit(TState from, TState? to, int delay) =>
			AddTransit(new TaskTransitWorker<TState>(from, to, (token) => Task.Delay(delay, token)));

		/// <summary>
		/// Must be invoked only in ctor! Adds transit worker into building statemachine that will do transit when created task returns true
		/// </summary>
		/// <param name="from">Start state</param>
		/// <param name="to">Target state</param>
		/// <param name="taskCreator">Delegate that will create tasks using CancellationToken</param>
		protected void AddTransit(TState from, TState? to, Func<CancellationToken, Task<bool>> taskCreator) =>
			AddTransit(new TaskTransitWorker<TState>(from, to, AwaitCycleTask(taskCreator)));

		/// <summary>
		/// Must be invoked only in ctor! Adds transit worker into building statemachine that will do transit when predicate returns true
		/// </summary>
		/// <param name="from">Start state</param>
		/// <param name="to">Target state</param>
		/// <param name="predicate">Predicate itself</param>
		protected void AddTransit(TState from, TState? to, Func<bool> predicate) =>
			AddTransit(new PredicateTransitWorker<TState>(from, to, predicate));

		/// <summary>
		/// Must be invoked only in ctor! Adds transit worker into building statemachine that will do transit when task will complited
		/// </summary>
		/// <param name="from">Start state</param>
		/// <param name="to">Target state</param>
		/// <param name="taskCreator">Delegate that will create tasks using CancellationToken</param>
		protected void AddTransit(TState from, TState? to, Func<CancellationToken, Task> taskCreator) =>
			AddTransit(new TaskTransitWorker<TState>(from, to, taskCreator));

		/// <summary>
		/// Registers handler for event that fired on startup and call handler if initial state equals given
		/// </summary>
		/// <param name="state">Target initial state</param>
		/// <param name="handler">Handler to register</param>
		protected void AddStartup(TState state, Action handler)
		{
			ThrowIfBuilt();
			if(startupHandlers.ContainsKey(state) == false) startupHandlers.Add(state, new());
			startupHandlers[state].Add(handler);
		}

		/// <summary>
		/// Register handler for event that fired on statemachine state changing and call handler if state equals given. It don't work with inital state on startup
		/// </summary>
		/// <param name="state">Target state</param>
		/// <param name="handler">Handler to register</param>
		protected void AddHandler(TState state, Action<TState> handler)
		{
			ThrowIfBuilt();
			stateHandlers.Add((old) =>
				{ if (GetStateMachine().CurrentState.Equals(state)) handler.Invoke(old); });
		}
	}
}
