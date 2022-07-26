using DidiFrame.Utils;
using DidiFrame.Utils.StateMachine;

namespace DidiFrame.Lifetimes
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
		private ILifetimeContext<TBase>? context;
		private readonly StateMachineBuilder<TState> smBuilder;
		private IStateMachine<TState>? machine;
		private bool hasBuilt = false;
		private Guid? guid;
		private IServer? server;
		private readonly Dictionary<TState, List<Action>> startupHandlers = new();
		private readonly List<Action<TState>> stateHandlers = new();
		private readonly WaitFor waitForCrash = new();


		protected bool IsNewlyCreated => GetContext().IsNewlyCreated;

		protected bool IsFinalized { get; private set; }

		public Guid Guid => guid ?? throw new InvalidOperationException("Enable to get GUID before starting");

		public IServer Server => server ?? throw new InvalidOperationException("Enable to get GUID before starting");


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.AbstractStateBasedLifetime`2
		/// </summary>
		/// <param name="baseObj">Base object of that lifetime</param>
		public AbstractStateBasedLifetime(ILogger logger)
		{
			smBuilder = new StateMachineBuilder<TState>(logger);
			smBuilder.AddStateTransitWorker(new ResetTransitWorker<TState>(null, waitForCrash.Await));
		}


		/// <summary>
		/// Provides change-safe and thread-safe access to base object, automaticly notify state updater and freeze state machine util return disposed
		/// </summary>
		/// <param name="smFreeze">Statemachine freeze information, it will be automaticly disposed</param>
		/// <returns>DidiFrame.Utils.ObjectHolder`1 objects that must be disposed after wrtings</returns>
		protected virtual ObjectHolder<TBase> GetBase(out FreezeModel<TState> smFreeze)
		{
			var smFreezeIn = GetStateMachine().Freeze();
			smFreeze = smFreezeIn;

			var baseObject = GetContext().AccessBase().Open();
			var startState = baseObject.Object.State;

			return new ObjectHolder<TBase>(baseObject.Object, (holder) =>
			{
				Exception? ex = null;

				if (!startState.Equals(baseObject.Object.State))
				{
					ex = new InvalidOperationException("Enable to change state in base object of lifetime. State reverted and object saved");
					baseObject.Object.State = startState;
				}

				baseObject.Dispose();
				smFreezeIn.Dispose();

				if (ex is not null) throw ex;
			});
		}

		/// <summary>
		/// Provides change-safe and thread-safe access to base object, automaticly notify state updater and freeze state machine util return disposed
		/// </summary>
		/// <returns>DidiFrame.Utils.ObjectHolder`1 objects that must be disposed after wrtings</returns>
		protected ObjectHolder<TBase> GetBase() => GetBase(out _);

		protected TTarget GetBaseProperty<TTarget>(Func<TBase, TTarget> selector)
		{
			var baseObj = GetReadOnlyBase();
			var value = selector(baseObj.Object);
			baseObj.Dispose();
			return value;
		}

		protected virtual ObjectHolder<TBase> GetReadOnlyBase()
		{
			var objectHolder = GetContext().AccessBase().Open();
			var baseObject = objectHolder.Object;
			var prevHashCode = baseObject.GetHashCode();

			return new ObjectHolder<TBase>(baseObject, _ =>
			{
				objectHolder.Dispose();
				if (prevHashCode != baseObject.GetHashCode())
				{
					var ex = new InvalidOperationException("Error, base was changed in readonly base accessor. Lifetime is collapsing");
					GetContext().CrashPipeline(ex, false);
					throw ex;
				}
			});
		}

		protected IObjectController<TBase> GetBaseController(bool asReadOnly = true)
		{
			return new BaseWrapController(this, asReadOnly);
		}

		/// <inheritdoc/>
		public void Run(TBase initinalBase, ILifetimeContext<TBase> context)
		{
			this.context = context;
			server = initinalBase.Server;
			guid = initinalBase.Guid;

			OnBuild(initinalBase);

			machine = smBuilder.Build();
			machine.StateChanged += Machine_StateChanged;
			hasBuilt = true;

			OnRun(initinalBase.State, initinalBase);

			foreach (var item in startupHandlers)
				foreach (var handler in item.Value) handler();

			machine.Start(initinalBase.State);
		}

		private void Machine_StateChanged(IStateMachine<TState> stateMahcine, TState oldState)
		{
			if (IsFinalized) return;

			var cs = stateMahcine.CurrentState;
			if (cs.HasValue == false)
			{
				FinalizeLifetime();
			}
			else
			{
				using (var b = GetContext().AccessBase().Open()) //It is not bug
				{
					b.Object.State = cs.Value;
				}

				foreach (var handler in stateHandlers) handler(oldState);
			}
		}

		/// <summary>
		/// Gets a cached lifetime updater instance. No manual updates required
		/// </summary>
		/// <returns>The cached updater</returns>
		/// <exception cref="InvalidOperationException">If invoked before Run() call (example in ctor)</exception>
		private ILifetimeContext<TBase> GetContext()
		{
			lock (this)
			{
				if (!hasBuilt)
					throw new InvalidOperationException("Enable to get context before starting");
				return context ?? throw new ImpossibleVariantException();
			}
		}

		protected void ThrowIfFinalized()
		{
			lock (this)
			{
				if (IsFinalized)
					throw new InvalidOperationException("Lifetime is finalized");
			}
		}

		protected abstract void OnBuild(TBase initialBase);

		private void FinalizeLifetime()
		{
			lock (this)
			{
				if (IsFinalized == false)
				{
					IsFinalized = true;
					OnFinalize();
					OnDispose();
					GetContext().FinalizeLifetime();
				}
			}
		}

		protected void CrashLifetime(Exception exception, bool isInvalidBase)
		{
			lock (this)
			{
				if (IsFinalized == false)
				{
					IsFinalized = true;
					OnDispose();
					GetContext().CrashPipeline(exception, isInvalidBase);
					waitForCrash.Callback();
				}
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
		protected virtual void OnRun(TState state, TBase initialBase)
		{

		}

		protected virtual void OnDispose()
		{

		}

		protected virtual void OnFinalize()
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
			if (startupHandlers.ContainsKey(state) == false) startupHandlers.Add(state, new());
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


		private class BaseWrapController : IObjectController<TBase>
		{
			private readonly AbstractStateBasedLifetime<TState, TBase> lifetime;
			private readonly bool asReadOnly;


			public BaseWrapController(AbstractStateBasedLifetime<TState, TBase> lifetime, bool asReadOnly)
			{
				this.lifetime = lifetime;
				this.asReadOnly = asReadOnly;
			}


			public ObjectHolder<TBase> Open()
			{
				return asReadOnly ? lifetime.GetReadOnlyBase() : lifetime.GetBase();
			}
		}
	}
}
