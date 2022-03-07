namespace CGZBot3.Systems.Reputation
{
	public interface ISettingsRepository
	{
		public ReputationSettings GetSettings(IServer server);
	}
}
