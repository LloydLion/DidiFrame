﻿using DidiFrame.Utils;
using DidiFrame.Utils.StateMachine;

namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Base class of statemachine-based lifetime
	/// </summary>
	/// <typeparam name="TState">Internal statemachine state type</typeparam>
	/// <typeparam name="TBase">Type of base object of that lifetime </typeparam>
	public abstract class AbstractStateBasedLifetime<TState, TBase> : AbstractLifetime<TBase>
		where TState : struct
		where TBase : class, IStateBasedLifetimeBase<TState>
	{
		private const string StateMachineField = "stateMachine";
		private readonly StateMachineBuilder<TState> smBuilder;


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.AbstractStateBasedLifetime`2
		/// </summary>
		/// <param name="logger">Logger for lifetime</param>
		protected AbstractStateBasedLifetime(ILogger logger) : base(logger)
		{
			smBuilder = new StateMachineBuilder<TState>(logger);

			LifetimeRan += OnRun;
			LifetimeUpdated += OnUpdate;
		}


		protected event StateChangedEventHandler<TState>? StateChanged;


		protected IStateMachine<TState> StateMachine => Data.Get<IStateMachine<TState>>(StateMachineField);
		

		/// <inheritdoc/>
		private void OnRun(TBase initialBase, ILifetimeContext<TBase> context, InitialDataBuilder builder)
		{
			PrepareStateMachine(initialBase, builder);

			var machine = smBuilder.Build();
			machine.StateChanged += OnStateChanged;

			builder.AddData(StateMachineField, machine);

			machine.Start(initialBase.State);
		}

		private void OnUpdate()
		{
			StateMachine.UpdateState();
		}

		private void OnStateChanged(IStateMachine<TState> stateMahcine, TState oldState)
		{
			var newState = stateMahcine.CurrentState;

			if (newState is null)
				FinalizeLifetime();
			else
			{
				using var b = base.GetBase(); //It is not bug
				b.Object.State = newState.Value;
			}

			StateChanged?.Invoke(stateMahcine, oldState);
		}

		protected sealed override ObjectHolder<TBase> GetBase() => GetBaseAndControlState().AsHolder();

		protected virtual StateControlledObjectHolder GetBaseAndControlState()
		{
			StateControlledObjectHolder? buffer = null;

			SynchronizationContext.Send(s =>
			{
				var baseBase = base.GetBase();
				buffer = new StateControlledObjectHolder(baseBase.Object, _ =>
				{
					baseBase.Dispose();
					return StateMachine.UpdateState();
				});
			}, null);

			return buffer ?? throw new ImpossibleVariantException();
		}

		protected abstract void PrepareStateMachine(TBase initialBase, InitialDataBuilder builder);

		#region SM transit creators
		private static Func<CancellationToken, Task> AwaitCycleTask(Func<CancellationToken, Task<bool>> taskCreator)
		{
			return async (token) =>
			{
				while (token.IsCancellationRequested == false && await taskCreator.Invoke(token) == false)
					await Task.Delay(100, token);
			};
		}

		/// <summary>
		/// Must be invoked only in ctor! Directly adds transit into building statemachine
		/// </summary>
		/// <param name="transit">Transit itself</param>
		/// <exception cref="InvalidOperationException">If invoked outside ctor (after Run() calling)</exception>
		protected void AddTransit(StateTransit<TState> transit)
		{
			ThrowIfBuilden();
			smBuilder.AddStateTransit(transit);
		}
		/// <summary>
		/// Must be invoked only in ctor! Directly adds transit worker into building statemachine
		/// </summary>
		/// <param name="worker">Transit worker</param>
		/// <param name="router">Transit router</param>
		/// <exception cref="InvalidOperationException">If invoked outside ctor (after Run() calling)</exception>
		protected void AddTransit(IStateTransitWorker<TState> worker, IStateTransitRouter<TState> router)
		{
			ThrowIfBuilden();
			smBuilder.AddStateTransit(worker, router);
		}

		/// <summary>
		/// Must be invoked only in ctor! Adds transit worker into building statemachine that will do transit after given timeout
		/// </summary>
		/// <param name="from">Start state</param>
		/// <param name="to">Target state</param>
		/// <param name="delay">Transit timeout</param>
		/// <exception cref="InvalidOperationException">If invoked outside ctor (after Run() calling)</exception>
		protected void AddTransit(TState from, TState? to, int delay) =>
			AddTransit(new TaskTransitWorker<TState>((token) => Task.Delay(delay, token)), new SimpleTranitRouter<TState>(from, to));

		/// <summary>
		/// Must be invoked only in ctor! Adds transit worker into building statemachine that will do transit when created task returns true
		/// </summary>
		/// <param name="from">Start state</param>
		/// <param name="to">Target state</param>
		/// <param name="taskCreator">Delegate that will create tasks using CancellationToken</param>
		protected void AddTransit(TState from, TState? to, Func<CancellationToken, Task<bool>> taskCreator) =>
			AddTransit(new TaskTransitWorker<TState>(AwaitCycleTask(taskCreator)), new SimpleTranitRouter<TState>(from, to));

		/// <summary>
		/// Must be invoked only in ctor! Adds transit worker into building statemachine that will do transit when predicate returns true
		/// </summary>
		/// <param name="from">Start state</param>
		/// <param name="to">Target state</param>
		/// <param name="predicate">Predicate itself</param>
		protected void AddTransit(TState from, TState? to, Func<bool> predicate) =>
			AddTransit(new PredicateTransitWorker<TState>(predicate), new SimpleTranitRouter<TState>(from, to));

		/// <summary>
		/// Must be invoked only in ctor! Adds transit worker into building statemachine that will do transit when task will complited
		/// </summary>
		/// <param name="from">Start state</param>
		/// <param name="to">Target state</param>
		/// <param name="taskCreator">Delegate that will create tasks using CancellationToken</param>
		protected void AddTransit(TState from, TState? to, Func<CancellationToken, Task> taskCreator) =>
			AddTransit(new TaskTransitWorker<TState>(taskCreator), new SimpleTranitRouter<TState>(from, to));
		#endregion


		protected sealed class StateControlledObjectHolder : IDisposable
		{
			private readonly ObjectHolder<TBase> holder;
			private StateUpdateResult<TState>? updateResult;
			private bool isDisposed;


			public StateControlledObjectHolder(TBase obj, Func<StateControlledObjectHolder, StateUpdateResult<TState>?> finalizer)
			{
				holder = new ObjectHolder<TBase>(obj, _ =>
				{
					updateResult = finalizer(this);
					isDisposed = true;
				});
			}


			public TBase Object => holder.Object;


			public StateUpdateResult<TState>? GetUpdateResult()
			{
				if (isDisposed == false)
					throw new InvalidOperationException("Enable to get update result before object hodler is disposed");

				return updateResult;
			}

			public ObjectHolder<TBase> AsHolder() => holder;

			public void Dispose() => holder.Dispose();
		}
	}
}
