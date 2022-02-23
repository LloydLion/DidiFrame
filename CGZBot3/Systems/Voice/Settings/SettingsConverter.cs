namespace CGZBot3.Systems.Voice.Settings
{
	internal class SettingsConverter : IModelConverter<VoiceSettingsPM, VoiceSettings>
	{
		public Task<VoiceSettingsPM> ConvertDownAsync(IServer server, VoiceSettings origin)
		{
			return Task.FromResult(new VoiceSettingsPM() { CreationCategory = origin.CreationCategory.Id, ReportChannel = origin.ReportChannel.Id });
		}

		public async Task<VoiceSettings> ConvertUpAsync(IServer server, VoiceSettingsPM pm)
		{
			return new VoiceSettings(await server.GetCategoryAsync(pm.CreationCategory), (await server.GetChannelAsync(pm.ReportChannel)).AsText());
		}
	}
}
