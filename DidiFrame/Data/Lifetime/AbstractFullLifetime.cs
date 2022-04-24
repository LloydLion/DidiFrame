using DidiFrame.Utils;
using DidiFrame.Utils.StateMachine;

namespace DidiFrame.Data.Lifetime
{
	public abstract class AbstractFullLifetime<TState, TBase> : AbstractStateBasedLifetime<TState, TBase>
		where TState : struct
		where TBase : class, IStateBasedLifetimeBase<TState>
	{
		private readonly WaitFor waitForClose = new();
		private MessageAliveHolder? optionalReport = null;
		private bool hasReport = false;
		private bool hasBuilt = false;


		public AbstractFullLifetime(IServiceProvider services, TBase baseObj) : base(services, baseObj)
		{
			AddTransit(new ResetTransitWorker<TState>(null, waitForClose.Await));
		}


		public void Close()
		{
			waitForClose.Callback();
		}

		protected new ObjectHolder<TBase> GetBase()
		{
			var bo = GetBase(out var smFreeze);

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

		protected virtual void OnRunInternal(TState state) { }

		//Will has subscribed only if has report
		private void OnReportCreated(IMessage obj) => GetUpdater().Update(this);

		//Will has subscribed only if has report
		private void OnStateChanged(IStateMachine<TState> sm, TState oldState)
		{
			if (GetStateMachine().CurrentState is null) GetReport().Dispose();
			else GetReport().Update().Wait();
		}

		/// <summary>
		/// Call once or don't call
		/// </summary>
		protected void AddReport(MessageAliveHolder holder)
		{
			if (hasBuilt) throw new InvalidOperationException("Object done, please don't invoke any constructing methods");

			optionalReport = holder;
			hasReport = true;
		}

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
