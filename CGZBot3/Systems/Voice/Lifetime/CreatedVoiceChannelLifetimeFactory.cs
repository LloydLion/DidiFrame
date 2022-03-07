namespace CGZBot3.Systems.Voice.Lifetime
{
	internal class CreatedVoiceChannelLifetimeFactory : ICreatedVoiceChannelLifetimeFactory
	{
		private readonly UIHelper helper;
		private readonly IServersSettingsRepository<VoiceSettings> settings;
		private readonly ILoggerFactory loggerFactory;


		public CreatedVoiceChannelLifetimeFactory(UIHelper helper, IServersSettingsRepositoryFactory settingsFactory, ILoggerFactory loggerFactory)
		{
			this.helper = helper;
			settings = settingsFactory.Create<VoiceSettings>(SettingsKeys.VoiceSystem);
			this.loggerFactory = loggerFactory;
		}


		public async Task<CreatedVoiceChannelLifetime> CreateAsync(CreatedVoiceChannel model, Action<CreatedVoiceChannelLifetime> endOfLifeCallback)
		{
			var report = await helper.SendReportAsync(model, settings.Get(model.BaseChannel.Server).ReportChannel);

			var lifetime = new CreatedVoiceChannelLifetime(model, loggerFactory.CreateLogger<CreatedVoiceChannelLifetime>(), report, endOfLifeCallback);

			return lifetime;
		}
	}
}
