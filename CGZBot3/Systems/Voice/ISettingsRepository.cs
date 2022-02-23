namespace CGZBot3.Systems.Voice
{
	public interface ISettingsRepository
	{
		public Task<VoiceSettings> GetSettingsAsync(IServer server);
	}
}
