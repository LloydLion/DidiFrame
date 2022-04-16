using CGZBot3.Data.Lifetime;
using CGZBot3.Entities.Message;
using CGZBot3.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Voice
{
	public class CreatedVoiceChannelLifetime : AbstractFullLifetime<VoiceChannelState, CreatedVoiceChannel>
	{
		private static readonly EventId ChannelDeleteErrorID = new(13, "ChannelDeleteError");


		private readonly ILogger<CreatedVoiceChannelLifetime> logger;
		private readonly UIHelper uiHelper;


		public CreatedVoiceChannelLifetime(CreatedVoiceChannel baseChannel, IServiceProvider services) : base(services, baseChannel)
		{
			logger = services.GetRequiredService<ILoggerFactory>().CreateLogger<CreatedVoiceChannelLifetime>();
			uiHelper = services.GetRequiredService<UIHelper>();

			AddReport(new MessageAliveHolder(baseChannel.ReportMessage, true, CreateReport, (_) => { }));


			AddTransit(VoiceChannelState.Timeout, VoiceChannelState.Alive, 10000);
			AddTransit(VoiceChannelState.Alive, null, AliveDisposingTransit);
		}


		protected async override void OnDispose()
		{
			var baseObj = GetBaseDirect();

			try { await baseObj.BaseChannel.DeleteAsync(); }
			catch (Exception ex) { logger.Log(LogLevel.Warning, ChannelDeleteErrorID, ex, "Enbale to delete created voice channel"); }
		}

		public async Task<bool> AliveDisposingTransit(CancellationToken token)
		{
			var baseObj = GetBaseDirect();

			await Task.Delay(1000, token);

			return baseObj.BaseChannel.ConnectedMembers.Count == 0;
		}

		private MessageSendModel CreateReport() => uiHelper.CreateReport(GetBaseDirect().Name, GetBaseDirect().Creator);
	}
}
