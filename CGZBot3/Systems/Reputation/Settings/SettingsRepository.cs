namespace CGZBot3.Systems.Reputation.Settings
{
	internal class SettingsRepository : ISettingsRepository
	{
		private readonly IServersSettingsRepository settings;


		public SettingsRepository(IServersSettingsRepository settings)
		{
			this.settings = settings;
		}


		public ReputationSettings GetSettings(IServer server)
		{
			return settings.Get<ReputationSettings>(server, SettingsKeys.ReputationSystem);
		}
	}
}
