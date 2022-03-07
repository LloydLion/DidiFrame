namespace CGZBot3.Systems.Test.Settings
{
	internal class TestSettingsRepository : ITestSettingsRepository
	{
		private readonly IServersSettingsRepository repository;


		public TestSettingsRepository(IServersSettingsRepository repository)
		{
			this.repository = repository;
		}


		public TestSettings GetSettings(IServer server)
		{
			return repository.Get<TestSettings>(server, SettingsKeys.TestSystem);
		}
	}
}
