using DidiFrame.Utils;
using DidiFrame.Utils.StateMachine;

namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Useful extension for DidiFrame.Lifetimes.AbstractStateBasedLifetime`2
	/// </summary>
	/// <typeparam name="TState">Internal statemachine state type</typeparam>
	/// <typeparam name="TBase">Type of base object of that lifetime </typeparam>
	public abstract class AbstractFullLifetime<TState, TBase> : AbstractStateBasedLifetime<TState, TBase>
		where TState : struct
		where TBase : class, IStateBasedLifetimeBase<TState>
	{
		private readonly WaitFor waitForClose = new();
		private MessageAliveHolder? optionalReport = null;
		private bool hasReport = false;
		private bool hasBuilt = false;


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.AbstractFullLifetime`2
		/// </summary>
		public AbstractFullLifetime(ILogger logger) : base(logger)
		{
			AddTransit(new ResetTransitWorker<TState>(null, waitForClose.Await));
		}


		/// <summary>
		/// Finilize and closes this lifetime. Call is async and cann't be tracked
		/// </summary>
		public void Close()
		{
			waitForClose.Callback();
		}

		/// <summary>
		/// Provides change-safe and thread-safe access to base object, automaticly notify state updater and freeze state machine util return disposed.
		/// This method is overrided by DidiFrame.Lifetimes.AbstractFullLifetime`2
		/// </summary>
		/// <returns>DidiFrame.Utils.ObjectHolder`1 objects that must be disposed after wrtings</returns>
		protected new ObjectHolder<TBase> GetBase()
		{
			var bo = base.GetBase(out var smFreeze);

			return new ObjectHolder<TBase>(bo.Object, async (_) =>
			{
				bo.Dispose();

				if (hasReport)
				{
					var update = smFreeze.GetResult();
					if (update.HasStateUpdated == false) await GetReport().Update();
				}
			});
		}

		/// <summary>
		/// Provides change-safe and thread-safe access to base object, automaticly notify state updater and freeze state machine util return disposed.
		/// This method is overrided by DidiFrame.Lifetimes.AbstractFullLifetime`2
		/// </summary>
		/// <param name="smFreeze">Statemachine freeze information, it will be automaticly disposed</param>
		/// <returns>DidiFrame.Utils.ObjectHolder`1 objects that must be disposed after wrtings</returns>
		protected new ObjectHolder<TBase> GetBase(out FreezeModel<TState> smFreeze)
		{
			var bo = base.GetBase(out var internalFreeze);
			smFreeze = internalFreeze;

			return new ObjectHolder<TBase>(bo.Object, async (_) =>
			{
				bo.Dispose();

				if (hasReport)
				{
					var update = internalFreeze.GetResult();
					if (update.HasStateUpdated == false) await GetReport().Update();
				}
			});
		}

		/// <summary>
		/// Don't override, it used by DidiFrame.Lifetimes.AbstractFullLifetime`2. If overriding is important use OnRunInternal(TState)
		/// </summary>
		/// <param name="state">Initial statemachine state</param>
		protected async override void OnRun(TState state)
		{
			hasBuilt = true;

			if (hasReport)
			{
				var report = GetReport();

				report.AutoMessageCreated += OnReportCreated;
				GetStateMachine().StateChanged += OnStateChanged;

				if (report.IsExist) report.ProcessMessage();
				else await report.CheckAsync();
			}

			OnRunInternal(state);
		}

		/// <summary>
		/// Event handler. Calls on start. You mustn't call base.OnRun(TState)
		/// </summary>
		/// <param name="state">Initial statemachine state</param>
		protected virtual void OnRunInternal(TState state) { }

		//Will has subscribed only if has report
		private void OnReportCreated(IMessage obj) => .Update(this);

		//Will has subscribed only if has report
		private void OnStateChanged(IStateMachine<TState> sm, TState oldState)
		{
			if (GetStateMachine().CurrentState is null) GetReport().Dispose();
			else GetReport().Update().Wait();
		}

		/// <summary>
		/// Adds automaticly maintaining report message for lifetime. It can be added only one time
		/// </summary>
		/// <param name="holder">Holder that was extracted from base object and contains all message info</param>
		/// <exception cref="InvalidOperationException">If called outside ctor (after Run() calling)</exception>
		protected void AddReport(MessageAliveHolder holder)
		{
			if (hasBuilt) throw new InvalidOperationException("Object done, please don't invoke any constructing methods");

			optionalReport = holder;
			hasReport = true;
		}

		/// <summary>
		/// Provides added report's alive holder
		/// </summary>
		/// <returns>Report's alive holder</returns>
		/// <exception cref="InvalidOperationException">If no report has added or if called in ctor (before Run() calling)</exception>
		protected MessageAliveHolder GetReport()
		{
			if (hasBuilt == false)
				throw new InvalidOperationException("Enable get report in ctor");

			if (optionalReport is null)
				throw new InvalidOperationException("No report has added");

			return optionalReport;
		}
	}
}
