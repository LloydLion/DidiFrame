namespace CGZBot3.Systems.Voice.Settings
{
	internal class SettingsRepository : ISettingsRepository
	{
		private readonly IServersSettingsRepository repository;


		public SettingsRepository(IServersSettingsRepository repository)
		{
			this.repository = repository;
		}


		public VoiceSettings GetSettings(IServer server)
		{
			return repository.Get<VoiceSettings>(server, SettingsKeys.VoiceSystem);
		}
	}
}
