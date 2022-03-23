using CGZBot3.Data.Lifetime;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Voice
{
	public class CreatedVoiceChannelLifetime : AbstractStateBasedLifetime<VoiceChannelState, CreatedVoiceChannel>
	{
		private static readonly EventId ReportDeleteErrorID = new(12, "ReportDeleteError");
		private static readonly EventId ChannelDeleteErrorID = new(13, "ChannelDeleteError");


		private readonly ILogger<CreatedVoiceChannelLifetime> logger;
		private readonly SystemCore sysCore;


		public CreatedVoiceChannelLifetime(CreatedVoiceChannel baseChannel, IServiceProvider services) : base(services, baseChannel)
		{
			logger = services.GetRequiredService<ILoggerFactory>().CreateLogger<CreatedVoiceChannelLifetime>();
			sysCore = services.GetRequiredService<SystemCore>();

			AddTransit(VoiceChannelState.Timeout, VoiceChannelState.Alive, 10000);
			AddTransit(VoiceChannelState.Alive, null, AliveDisposingTransit);
		}

		protected override void OnDispose()
		{
			var baseObj = GetBaseDirect();

			Task.WaitAll(deleteChannel(baseObj), deleteMessage(baseObj));


			async Task deleteChannel(CreatedVoiceChannel baseObj)
			{
				try { await baseObj.BaseChannel.DeleteAsync(); }
				catch (Exception ex) { logger.Log(LogLevel.Warning, ChannelDeleteErrorID, ex, "Enbale to delete created voice channel"); }
			}

			async Task deleteMessage(CreatedVoiceChannel baseObj)
			{
				try { await baseObj.ReportMessage.DeleteAsync(); }
				catch (Exception ex) { logger.Log(LogLevel.Warning, ReportDeleteErrorID, ex, "Enbale to delete voice channel report"); }
			}
		}

		public async Task<bool> AliveDisposingTransit(CancellationToken token)
		{
			var baseObj = GetBaseDirect();

			await Task.Delay(1000, token);

			return baseObj.BaseChannel.ConnectedMembers.Count == 0;
		}

		protected override void OnRun(VoiceChannelState state)
		{
			sysCore.FixChannelLifetimeAsync(this).Wait();
		}
	}
}
