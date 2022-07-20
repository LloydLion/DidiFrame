using DidiFrame.Lifetimes;
using DidiFrame.Utils;

namespace TestBot.Systems.Voice
{
	public class CreatedVoiceChannelLifetime : AbstractFullLifetime<VoiceChannelState, CreatedVoiceChannel>
	{
		private static readonly EventId ChannelDeleteErrorID = new(13, "ChannelDeleteError");


		private readonly ILogger<CreatedVoiceChannelLifetime> logger;
		private readonly UIHelper uiHelper;


		public CreatedVoiceChannelLifetime(ILogger<CreatedVoiceChannelLifetime> logger, UIHelper uiHelper) : base(logger)
		{
			this.logger = logger;
			this.uiHelper = uiHelper;
		}


		protected async override void OnDispose()
		{
			using var baseObj = GetBase();

			try { await baseObj.Object.BaseChannel.DeleteAsync(); }
			catch (Exception ex) { logger.Log(LogLevel.Warning, ChannelDeleteErrorID, ex, "Enbale to delete created voice channel"); }
		}

		public async Task<bool> AliveDisposingTransit(CancellationToken token)
		{
			await Task.Delay(1000, token);

			return GetBaseAuto(s => s.BaseChannel.ConnectedMembers.Count) == 0;
		}

		private MessageSendModel CreateReport() => uiHelper.CreateReport(GetBaseAuto(s => s.Name), GetBaseAuto(s => s.Creator));

		public IMember GetCreator() => GetBaseAuto(s => s.Creator);

		public string GetName() => GetBaseAuto(s => s.Name);

		protected override void OnBuild(CreatedVoiceChannel initialBase, ILifetimeContext<CreatedVoiceChannel> context)
		{
			var controller = new SelectObjectContoller<CreatedVoiceChannel, MessageAliveHolder.Model>(context.AccessBase(), s => s.ReportMessage);
			AddReport(new MessageAliveHolder(controller, true, CreateReport, (_) => { }));

			AddTransit(VoiceChannelState.Timeout, VoiceChannelState.Alive, 10000);
			AddTransit(VoiceChannelState.Alive, null, AliveDisposingTransit);
		}
	}
}
