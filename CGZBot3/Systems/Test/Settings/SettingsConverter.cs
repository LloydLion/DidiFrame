namespace CGZBot3.Systems.Test.Settings
{
	internal class SettingsConverter : ISettingsConverter<TestSettingsRM, TestSettings>
	{
		public async Task<TestSettings> ConvertUpAsync(IServer server, TestSettingsRM db)
		{
			var channel = (ITextChannel)await server.GetChannelAsync(db.TestChannel);
			return new(db.SomeString, channel);
		}

		public Task<TestSettingsRM> ConvertDownAsync(IServer server, TestSettings origin)
		{
			return Task.FromResult(new TestSettingsRM() { SomeString = origin.SomeString, TestChannel = origin.TestChannel.Id });
		}
	}
}
