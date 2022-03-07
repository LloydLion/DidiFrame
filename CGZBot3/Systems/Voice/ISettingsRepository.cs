namespace CGZBot3.Systems.Voice
{
	public interface ISettingsRepository
	{
		public VoiceSettings GetSettings(IServer server);
	}
}
