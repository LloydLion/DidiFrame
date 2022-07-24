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


		protected async override void OnDisposeInternal()
		{
			using var baseObj = GetReadOnlyBase();

			try { await baseObj.Object.BaseChannel.DeleteAsync(); }
			catch (Exception ex) { logger.Log(LogLevel.Warning, ChannelDeleteErrorID, ex, "Enbale to delete created voice channel"); }
		}

		public async Task<bool> AliveDisposingTransit(CancellationToken token)
		{
			await Task.Delay(1000, token);

			return GetBaseProperty(s => s.BaseChannel.ConnectedMembers.Count) == 0;
		}

		private MessageSendModel CreateReport(object? parameter)
		{
			using var baseObj = WrapOrGetReadOnlyBase((CreatedVoiceChannel?)parameter);
			return uiHelper.CreateReport(baseObj.Object.Name, baseObj.Object.Creator);
		}

		public IMember GetCreator() => GetBaseProperty(s => s.Creator);

		public string GetName() => GetBaseProperty(s => s.Name);

		protected override void OnBuild(CreatedVoiceChannel initialBase)
		{
			AddReport(new MessageAliveHolder(initialBase.ReportMessage, parameter => WrapOrGetReadOnlyBase((CreatedVoiceChannel?)parameter).SelectHolder(s => s.ReportMessage), true, CreateReport, (_1, _2) => { }));

			AddTransit(VoiceChannelState.Timeout, VoiceChannelState.Alive, 10000);
			AddTransit(VoiceChannelState.Alive, null, AliveDisposingTransit);
		}
	}
}
