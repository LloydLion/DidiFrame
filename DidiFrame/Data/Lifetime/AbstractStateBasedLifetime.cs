﻿using DidiFrame.Utils;
using DidiFrame.Utils.StateMachine;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Data.Lifetime
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
		private readonly List<Action<TState>> stateHandlers = new();
		private static readonly ThreadLocker<AbstractStateBasedLifetime<TState, TBase>> baseLocker = new();


		public AbstractStateBasedLifetime(IServiceProvider services, TBase baseObj)
		{
			this.baseObj = baseObj;

			var tname = GetType().FullName ?? throw new ImpossibleVariantException();
			smBuilder = services.GetRequiredService<IStateMachineBuilderFactory<TState>>().Create(tname);
		}


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

		protected ObjectHolder<TBase> GetBase() => GetBase(out _);

		public TBase GetBaseClone()
		{
			using (baseLocker.Lock(this))
			{
				return (TBase)baseObj.Clone();
			}
		}

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

		protected ILifetimeStateUpdater<TBase> GetUpdater()
		{
			if (!hasBuilt)
				throw new InvalidOperationException("Enable to get updater before starting");
			return updater ?? throw new ImpossibleVariantException();
		}

		/// <summary>
		/// WARNING! Returned object is readonly!
		/// If changed changed will not writen to state
		/// </summary>
		/// <returns></returns>
		protected TBase GetBaseDirect()
		{
			using (baseLocker.Lock(this))
			{
				return baseObj;
			}
		}

		protected IStateMachine<TState> GetStateMachine()
		{
			if (!hasBuilt)
				throw new InvalidOperationException("Enable to get state machine before starting");
			return machine ?? throw new ImpossibleVariantException();
		}

		protected virtual void OnRun(TState state)
		{

		}

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

		protected void AddTransit(TState from, TState? to, Func<CancellationToken, Task> taskCreator) =>
			AddTransit(new TaskTransitWorker<TState>(from, to, taskCreator));

		protected void AddStartup(TState state, Action handler)
		{
			ThrowIfBuilt();
			if(startupHandlers.ContainsKey(state) == false) startupHandlers.Add(state, new());
			startupHandlers[state].Add(handler);
		}

		protected void AddHandler(TState state, Action<TState> handler)
		{
			ThrowIfBuilt();
			stateHandlers.Add((old) =>
				{ if (GetStateMachine().CurrentState.Equals(state)) handler.Invoke(old); });
		}
	}
}