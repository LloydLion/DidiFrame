namespace CGZBot3.Systems.Test.Settings
{
	internal class SettingsConverter : ISettingsConverter<TestSettingsDB, TestSettings>
	{
		private readonly IServersSettingsRepository repository;


		public SettingsConverter(IServersSettingsRepository repository)
		{
			this.repository = repository;
		}
		
	
		public async Task<TestSettings> ConvertUpAsync(IServer server, TestSettingsDB db)
		{
			var channel = (ITextChannel)await server.GetChannelAsync(db.TestChannel);
			return new(db.SomeString, channel, db.Id);
		}

		public async Task<TestSettingsDB> ConvertDownAsync(IServer server, TestSettings origin)
		{
			var settings = await repository.GetOrCreateAsync(server);
			var sysSets = settings.TestSystem ??= new TestSettingsDB();
			sysSets.SomeString = origin.SomeString;
			sysSets.TestChannel = origin.TestChannel.Id;

			return sysSets;
		}
	}
}
