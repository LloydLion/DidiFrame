namespace CGZBot3.Systems.Test.Settings
{
	internal class TestSettingsRepository : ITestSettingsRepository
	{
		private readonly IServersSettingsRepository repository;
		private readonly IModelConverter<TestSettingsPM, TestSettings> converter;


		public TestSettingsRepository(IServersSettingsRepository repository, IModelConverter<TestSettingsPM, TestSettings> converter)
		{
			this.repository = repository;
			this.converter = converter;
		}


		public async Task<TestSettings> GetSettingsAsync(IServer server)
		{
			var pm = repository.Get<TestSettingsPM>(server, SettingsKeys.TestSystem);

			return await converter.ConvertUpAsync(server, pm);
		}
	}
}
