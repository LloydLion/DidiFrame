namespace CGZBot3.Systems.Test.Settings
{
	internal class TestSettingsRepository : ITestSettingsRepository
	{
		private readonly IServersSettingsRepository repository;
		private readonly ISettingsConverter<TestSettingsDB, TestSettings> converter;


		public TestSettingsRepository(IServersSettingsRepository repository, ISettingsConverter<TestSettingsDB, TestSettings> converter)
		{
			this.repository = repository;
			this.converter = converter;
		}


		public async Task<TestSettings> GetSettingsAsync(IServer server)
		{
			var sets = await repository.GetOrCreateAsync(server, nameof(ServerSettings.TestSystem));
			var lsets = sets.TestSystem ?? throw new InvalidSettingsException("property TestSystem contains invalid data", new NullReferenceException("Value was null"));

			return await converter.ConvertUpAsync(server, lsets);
		}
	}
}
