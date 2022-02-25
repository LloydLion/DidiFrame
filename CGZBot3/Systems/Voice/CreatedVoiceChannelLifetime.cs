namespace CGZBot3.Systems.Voice
{
	public class CreatedVoiceChannelLifetime
	{
		private static readonly EventId ReportDeleteErrorID = new(12, "ReportDeleteError");
		private static readonly EventId ChannelDeleteErrorID = new(13, "ChannelDeleteError");


		private readonly CreatedVoiceChannel baseChannel;
		private readonly IMessage report;
		private readonly CancellationTokenSource cts = new();
		private bool isClosed = false;


		public CreatedVoiceChannelLifetime(CreatedVoiceChannel baseChannel, ILogger<CreatedVoiceChannelLifetime> logger, IMessage report, Action<CreatedVoiceChannelLifetime> endOfLifeCallback)
		{
			this.baseChannel = baseChannel;
			this.report = report;

			var token = cts.Token;
			var channel = baseChannel.BaseChannel;
			var server = channel.Server;

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

				endOfLifeCallback(this);

				try { baseChannel.BaseChannel.DeleteAsync().Wait(); }
				catch (Exception ex) { logger.Log(LogLevel.Warning, ReportDeleteErrorID, ex, "(server:{ServerId} channel:{ChannalId}) Can't delete report message", server.Id, channel.Id); }

				try { report.DeleteAsync().Wait(); }
				catch (Exception ex) { logger.Log(LogLevel.Warning, ChannelDeleteErrorID, ex, "(server:{ServerId} channel:{ChannalId}) Can't delete channel", server.Id, channel.Id); }
			}, token);
		}


		public bool IsClosed => isClosed;

		public CreatedVoiceChannel BaseObject => baseChannel;

		public Task CloseTask { get; }

		public IMessage Report => report;
	}
}
