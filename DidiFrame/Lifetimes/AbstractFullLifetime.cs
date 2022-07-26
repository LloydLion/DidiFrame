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
		private static readonly EventId FailedToUpdateReportID = new(22, "FailedToUpdateReport");
		private static readonly EventId FailedToDeleteReportID = new(23, "FailedToDeleteReport");
		private static readonly EventId FailedToRestoreReportID = new(24, "FailedToRestoreReport");


		private readonly WaitFor waitForClose = new();
		private readonly ILogger logger;
		private MessageAliveHolder<TBase>? optionalReport = null;
		private bool hasReport = false;
		private bool hasBuilt = false;


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.AbstractFullLifetime`2
		/// </summary>
		public AbstractFullLifetime(ILogger logger) : base(logger)
		{
			AddTransit(new ResetTransitWorker<TState>(null, waitForClose.Await));
			this.logger = logger;
		}


		/// <summary>
		/// Finilizes and closes this lifetime
		/// </summary>
		public Task CloseAsync()
		{
			waitForClose.Callback();
			return GetStateMachine().AwaitForState(null);
		}

		protected void Terminate(string reason = "Manual termination", Exception? exception = null)
		{
			CrashLifetime(new LifetimeTerminatedException(GetType(), Guid, reason, exception), false);
		}

		/// <summary>
		/// Provides change-safe and thread-safe access to base object, automaticly notify state updater and freeze state machine util return disposed.
		/// This method is overrided by DidiFrame.Lifetimes.AbstractFullLifetime`2
		/// </summary>
		/// <param name="smFreeze">Statemachine freeze information, it will be automaticly disposed</param>
		/// <returns>DidiFrame.Utils.ObjectHolder`1 objects that must be disposed after wrtings</returns>
		protected override ObjectHolder<TBase> GetBase(out FreezeModel<TState> smFreeze)
		{
			var bo = base.GetBase(out var internalFreeze);
			smFreeze = internalFreeze;

			return new ObjectHolder<TBase>(bo.Object, async (_) =>
			{
				bo.Dispose();

				if (hasReport && internalFreeze.GetResult().HasStateUpdated == false)
				{
					try
					{
						using var baseObj = GetReadOnlyBase();
						await GetReport().UpdateAsync(baseObj.Object);
					}
					catch (Exception ex)
					{
						logger.Log(LogLevel.Error, FailedToUpdateReportID, ex, "Enable to update report message for lifetime with id {LifetimeId}", Guid);
					}
				}
			});
		}

		protected ObjectHolder<TBase> GetBaseForReport() => base.GetBase(out _);

		protected override ObjectHolder<TBase> GetReadOnlyBase()
		{
			return base.GetReadOnlyBase();
		}

		/// <summary>
		/// Don't override, it used by DidiFrame.Lifetimes.AbstractFullLifetime`2. If overriding is important use OnRunInternal(TState)
		/// </summary>
		/// <param name="state">Initial statemachine state</param>
		protected override void OnRun(TState state, TBase initalBase)
		{
			hasBuilt = true;

			if (Server.IsClosed)
			{
				throw new ArgumentException("Target server has been closed");
			}

			if (hasReport)
			{
				var report = GetReport();

				GetStateMachine().StateChanged += OnStateChanged;

				var channel = report.GetChannel(initalBase);

				if (channel.IsExist == false)
				{
					throw new ArgumentException($"Target channel ({channel.Id}) for report has been closed");
				}

				channel.MessageDeleted += AbstractFullLifetime_MessageDeleted;
				channel.Server.ChannelDeleted += Server_ChannelDeleted;

				report.StartupAsync(initalBase).Wait();
			}

			Server.Client.ServerRemoved += Client_ServerRemoved;

			OnRunInternal(state);
		}

		//Will only subscribed if has report
		private void Server_ChannelDeleted(IServer server, ulong objectId)
		{
			bool ok;

			using (var baseObj = GetReadOnlyBase())
			{
				var report = GetReport();
				ok = report.GetChannel(baseObj.Object).Id == objectId;
			}

			if (ok)
			{
				Terminate("Report channel closed");
			}
		}

		private void Client_ServerRemoved(IServer server)
		{
			if (Server.Equals(server))
			{
				Terminate("Server closed");
			}
		}

		private async void AbstractFullLifetime_MessageDeleted(IClient sender, ITextChannelBase textChannel, ulong messageId)
		{
			try
			{
				using var baseObj = GetBaseForReport();
				await GetReport().OnMessageDeleted(baseObj.Object, textChannel, messageId);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, FailedToRestoreReportID, ex, "Enable to restore report message for lifetime with id {LifetimeId}", Guid);
			}
		}

		//Will be subscribed only if has report
		private async void OnStateChanged(IStateMachine<TState> stateMahcine, TState oldState)
		{
			if (stateMahcine.CurrentState is null) return;

			try
			{
				using var baseObj = GetReadOnlyBase();
				await GetReport().UpdateAsync(baseObj.Object);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, FailedToUpdateReportID, ex, "Enable to update report message for lifetime with id {LifetimeId}", Guid);
			}
		}

		protected async override void OnDispose()
		{
			Server.Client.ServerRemoved -= Client_ServerRemoved;

			Task? reportFinalizeTask = null;

			if (hasReport)
			{
				try
				{
					using var holder = GetReadOnlyBase();
					var report = GetReport();

					var channel = report.GetChannel(holder.Object);
					channel.MessageDeleted -= AbstractFullLifetime_MessageDeleted;
					channel.Server.ChannelDeleted -= Server_ChannelDeleted;
					reportFinalizeTask = report.FinalizeAsync(holder.Object);
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, FailedToDeleteReportID, ex, "Enable to delete report message for lifetime with id {Guid}", Guid);
				}
			}

			OnDisposeInternal();

			if (reportFinalizeTask is not null) await reportFinalizeTask;
		}

		/// <summary>
		/// Event handler. Calls on start. You mustn't call base.OnRun(TState)
		/// </summary>
		/// <param name="state">Initial statemachine state</param>
		protected virtual void OnRunInternal(TState state) { }

		protected virtual void OnDisposeInternal() { }

		/// <summary>
		/// Adds automaticly maintaining report message for lifetime. It can be added only one time
		/// </summary>
		/// <param name="holder">Holder that was extracted from base object and contains all message info</param>
		/// <exception cref="InvalidOperationException">If called outside ctor (after Run() calling)</exception>
		protected void AddReport(MessageAliveHolder<TBase> holder)
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
		protected MessageAliveHolder<TBase> GetReport()
		{
			if (hasBuilt == false)
				throw new InvalidOperationException("Enable get report in ctor");

			if (optionalReport is null)
				throw new InvalidOperationException("No report has added");

			return optionalReport;
		}
	}
}
