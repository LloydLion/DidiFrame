namespace CGZBot3.Systems.Reputation
{
	public interface ISettingsRepository
	{
		public Task<ReputationSettings> GetSettingsAsync(IServer server);
	}
}
