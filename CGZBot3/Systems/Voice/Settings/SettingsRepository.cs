namespace CGZBot3.Systems.Voice.Settings
{
	internal class SettingsRepository : ISettingsRepository
	{
		private readonly IServersSettingsRepository repository;
		private readonly IModelConverter<VoiceSettingsPM, VoiceSettings> converter;


		public SettingsRepository(IServersSettingsRepository repository, IModelConverter<VoiceSettingsPM, VoiceSettings> converter)
		{
			this.repository = repository;
			this.converter = converter;
		}


		public async Task<VoiceSettings> GetSettingsAsync(IServer server)
		{
			var pm = repository.Get<VoiceSettingsPM>(server, SettingsKeys.VoiceSystem);
			return await converter.ConvertUpAsync(server, pm);	
		}
	}
}
