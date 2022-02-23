namespace CGZBot3.Systems.Voice
{
	public class CreatedVoiceChannelLifetime : IDisposable
	{
		public const string ClosedState = "Closed";
		public const string RunningState = "Running";

		private static readonly EventId ReportDeleteErrorID = new(12, "ReportDeleteError");
		private static readonly EventId ChannelDeleteErrorID = new(13, "ChannelDeleteError");


		private readonly CreatedVoiceChannel baseChannel;
		private readonly IMessage report;
		private readonly CancellationTokenSource cts = new();
		private bool isClosed = false;


		public CreatedVoiceChannelLifetime(CreatedVoiceChannel baseChannel, ILogger logger, IDisposable loggingScope, IMessage report, Func<CreatedVoiceChannelLifetime, Task> endOfLifeAsyncCallback)
		{
			this.baseChannel = baseChannel;
			this.report = report;
			var token = cts.Token;

			if (baseChannel.StateString == RunningState)
			{
				CloseTask = Task.Run(() =>
				{
					var voice = baseChannel.BaseChannel;
					Thread.Sleep(10000);
					while (voice.ConnectedMembers.Count != 0)
					{
						Thread.Sleep(100);
						if (token.IsCancellationRequested) return;
					}

					isClosed = true;

					endOfLifeAsyncCallback(this).Wait();

					try { baseChannel.BaseChannel.DeleteAsync().Wait(); }
					catch (Exception ex) { logger.Log(LogLevel.Warning, ReportDeleteErrorID, ex, "Can't delete report message"); }

					try { report.DeleteAsync().Wait(); }
					catch (Exception ex) { logger.Log(LogLevel.Warning, ChannelDeleteErrorID, ex, "Can't delete channel"); }

					loggingScope.Dispose();
				}, token);
			}
			else
			{
				CloseTask = Task.CompletedTask;
				isClosed = true;
			}
		}


		public bool IsClosed => isClosed;

		public CreatedVoiceChannel BaseObject => baseChannel;

		public Task CloseTask { get; }

		public IMessage Report => report;


		public void Dispose()
		{
			cts.Cancel();
			CloseTask.Wait();
			SaveState();
		}

		public void SaveState()
		{
			baseChannel.StateString = isClosed ? ClosedState : RunningState;
		}
	}
}
