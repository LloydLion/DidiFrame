namespace CGZBot3.Systems.Reputation.Settings
{
	internal class SettingsRepository : ISettingsRepository
	{
		private readonly IServersSettingsRepository settings;
		private readonly IModelConverter<ReputationSettingsPM, ReputationSettings> converter;


		public SettingsRepository(IServersSettingsRepository settings, IModelConverter<ReputationSettingsPM, ReputationSettings> converter)
		{
			this.settings = settings;
			this.converter = converter;
		}


		public Task<ReputationSettings> GetSettingsAsync(IServer server)
		{
			var pm = settings.Get<ReputationSettingsPM>(server, SettingsKeys.ReputationSystem);
			return converter.ConvertUpAsync(server, pm);
		}
	}
}
