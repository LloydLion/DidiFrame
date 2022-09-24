using DidiFrame.Utils;
using DidiFrame.Utils.StateMachine;
using static DidiFrame.Lifetimes.AbstractFullLifetimeStatic;

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
		private const string ReportField = "report";
		private readonly WaitFor waitForClose = new();


		/// <summary>
		/// If lifetime has report message
		/// </summary>
		protected bool HasReport => Data.AddititionalData.ContainsKey(ReportField);


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.AbstractFullLifetime`2
		/// </summary>
		protected AbstractFullLifetime(ILogger logger) : base(logger)
		{
			AddTransit(new TaskTransitWorker<TState>(waitForClose.Await), new ResetTransitRouter<TState>());
			LifetimePostRan += OnPostRun;
		}


		/// <summary>
		/// Finilizes and closes this lifetime
		/// </summary>
		public Task CloseAsync()
		{
			waitForClose.Callback();
			return StateMachine.AwaitForState(null);
		}

		/// <inheritdoc/>
		protected override StateControlledObjectHolder GetBaseAndControlState()
		{
			StateControlledObjectHolder? buffer = null;
			SynchronizationContext.Send(s =>
			{
				var bo = base.GetBaseAndControlState();
				buffer = new StateControlledObjectHolder(bo.Object, _ =>
				{
					bo.Dispose();

					//If bo.GetUpdateResult() is not null report already updated in state machine state changed event handler
					if (HasReport && bo.GetUpdateResult() is null)
					{
						Task.Run(() =>
						{
							try
							{
								using var baseObj = GetReadOnlyBase();
								GetReport().UpdateAsync(baseObj.Object).Wait();
							}
							catch (Exception ex)
							{
								Logger.Log(LogLevel.Error, FailedToUpdateReportID, ex, "Enable to update report message for lifetime with id {LifetimeId}", Id);
							}
						});
					}

					return bo.GetUpdateResult();
				});
			}, null);

			return buffer ?? throw new ImpossibleVariantException();
		}

		/// <summary>
		/// Don't override, it used by DidiFrame.Lifetimes.AbstractFullLifetime`2. If overriding is important use OnRunInternal(TState)
		/// </summary>
		/// <param name="initialBase">Initial value of base, cannot be saved in lifetime</param>
		/// <param name="initialData">Initial lifetime parameters</param>
		protected void OnPostRun(TBase initialBase, InitialData initialData)
		{
			if (HasReport)
			{
				var report = GetReport();

				StateMachine.StateChanged += OnStateChanged;

				var channel = report.GetChannel(initialBase);

				if (channel.IsExist == false)
				{
					throw new ArgumentException($"Target channel ({channel.Id}) for report has been closed");
				}

				channel.MessageDeleted += OnMessageDeleted;
				channel.Server.ChannelDeleted += OnChannelDeleted;

				report.StartupAsync(initialBase).Wait();
			}
		}

		//Will only subscribed if has report
		private void OnChannelDeleted(IServer server, ulong objectId)
		{
			bool isDeleted;

			using (var baseObj = GetReadOnlyBase())
			{
				var report = GetReport();
				isDeleted = report.GetChannel(baseObj.Object).Id == objectId;
			}

			if (isDeleted)
			{
				TerminateLifetime("Report channel closed");
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
				Logger.Log(LogLevel.Error, FailedToUpdateReportID, ex, "Enable to update report message for lifetime with id {LifetimeId}", Id);
			}
		}

		//Will be subscribed only if has report
		private async void OnMessageDeleted(IClient sender, ITextChannelBase textChannel, ulong messageId)
		{
			try
			{
				using var baseObj = base.GetBase();
				await GetReport().OnMessageDeleted(baseObj.Object, textChannel, messageId);
			}
			catch (Exception ex)
			{
				Logger.Log(LogLevel.Error, FailedToRestoreReportID, ex, "Enable to restore report message for lifetime with id {LifetimeId}", Id);
			}
		}

		/// <inheritdoc/>
		public override async void Dispose()
		{
			GC.SuppressFinalize(this);

			if (HasReport)
			{
				try
				{
					using var holder = GetReadOnlyBase();
					var report = GetReport();

					var channel = report.GetChannel(holder.Object);
					channel.MessageDeleted -= OnMessageDeleted;
					channel.Server.ChannelDeleted -= OnChannelDeleted;
					await report.FinalizeAsync(holder.Object);
				}
				catch (Exception ex)
				{
					Logger.Log(LogLevel.Error, FailedToDeleteReportID, ex, "Enable to delete report message for lifetime with id {Guid}", Id);
				}
			}
		}

		/// <summary>
		/// Adds automaticly maintaining report message for lifetime. It can be added only one time
		/// </summary>
		/// <param name="holder">Holder that was extracted from base object and contains all message info</param>
		/// <param name="builder">Lifetime's initial data builder to add report</param>
		/// <exception cref="InvalidOperationException">If called outside ctor (after Run() calling)</exception>
		protected void AddReport(MessageAliveHolder<TBase> holder, InitialDataBuilder builder)
		{
			ThrowIfBuilden();

			builder.AddData(ReportField, holder);
		}

		/// <summary>
		/// Provides added report's alive holder
		/// </summary>
		/// <returns>Report's alive holder</returns>
		/// <exception cref="InvalidOperationException">If no report has added or if called in ctor (before Run() calling)</exception>
		protected MessageAliveHolder<TBase> GetReport()
		{
			if (HasReport == false)
				throw new InvalidOperationException("No report has added");

			return Data.Get<MessageAliveHolder<TBase>>(ReportField);
		}
	}

	internal static class AbstractFullLifetimeStatic
	{
		public static readonly EventId FailedToUpdateReportID = new(22, "FailedToUpdateReport");
		public static readonly EventId FailedToDeleteReportID = new(23, "FailedToDeleteReport");
		public static readonly EventId FailedToRestoreReportID = new(24, "FailedToRestoreReport");
	}
}
