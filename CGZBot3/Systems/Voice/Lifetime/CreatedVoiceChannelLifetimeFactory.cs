namespace CGZBot3.Systems.Voice.Lifetime
{
	internal class CreatedVoiceChannelLifetimeFactory : ICreatedVoiceChannelLifetimeFactory
	{
		private readonly UIHelper helper;
		private readonly ISettingsRepository settings;
		private readonly ILoggerFactory loggerFactory;


		public CreatedVoiceChannelLifetimeFactory(UIHelper helper, ISettingsRepository settings, ILoggerFactory loggerFactory)
		{
			this.helper = helper;
			this.settings = settings;
			this.loggerFactory = loggerFactory;
		}


		public async Task<CreatedVoiceChannelLifetime> CreateAsync(CreatedVoiceChannel model, Action<CreatedVoiceChannelLifetime> endOfLifeCallback)
		{
			var report = await helper.SendReportAsync(model, (await settings.GetSettingsAsync(model.BaseChannel.Server)).ReportChannel);

			var lifetime = new CreatedVoiceChannelLifetime(model, loggerFactory.CreateLogger<CreatedVoiceChannelLifetime>(), report, endOfLifeCallback);

			return lifetime;
		}
	}
}
