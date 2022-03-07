namespace CGZBot3.Systems.Test.Settings
{
	internal interface ITestSettingsRepository
	{
		public TestSettings GetSettings(IServer server);
	}
}
