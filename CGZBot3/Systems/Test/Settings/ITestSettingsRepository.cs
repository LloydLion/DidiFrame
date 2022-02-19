namespace CGZBot3.Systems.Test.Settings
{
	internal interface ITestSettingsRepository
	{
		public Task<TestSettings> GetSettingsAsync(IServer server);
	}
}
