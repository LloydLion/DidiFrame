namespace CGZBot3.Systems.Test.Settings
{
	internal class TestSettingsRepository : ITestSettingsRepository
	{
		private readonly IServersSettingsRepository repository;
		private readonly ISettingsConverter<TestSettingsRM, TestSettings> converter;


		public TestSettingsRepository(IServersSettingsRepository repository, ISettingsConverter<TestSettingsRM, TestSettings> converter)
		{
			this.repository = repository;
			this.converter = converter;
		}


		public async Task<TestSettings> GetSettingsAsync(IServer server)
		{
			var pm = repository.GetOrCreate<TestSettingsRM>(server, SystemsKeys.TestSystem);

			return await converter.ConvertUpAsync(server, pm);
		}
	}
}
