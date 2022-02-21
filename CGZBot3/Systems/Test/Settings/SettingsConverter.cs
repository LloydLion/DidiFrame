namespace CGZBot3.Systems.Test.Settings
{
	internal class SettingsConverter : IModelConverter<TestSettingsPM, TestSettings>
	{
		public async Task<TestSettings> ConvertUpAsync(IServer server, TestSettingsPM pm)
		{
			var channel = (ITextChannel)await server.GetChannelAsync(pm.TestChannel);
			return new(pm.SomeString, channel);
		}

		public Task<TestSettingsPM> ConvertDownAsync(IServer server, TestSettings origin)
		{
			return Task.FromResult(new TestSettingsPM() { SomeString = origin.SomeString, TestChannel = origin.TestChannel.Id });
		}
	}
}
