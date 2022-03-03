namespace CGZBot3.Systems.Reputation.Settings
{
	internal class SettingsConverter : IModelConverter<ReputationSettingsPM, ReputationSettings>
	{
		public Task<ReputationSettingsPM> ConvertDownAsync(IServer server, ReputationSettings origin)
		{
			return Task.FromResult(new ReputationSettingsPM()
			{
				Sources = new ReputationSettingsPM.Source()
					{ VoiceCreation = origin.Sources.VoiceCreation, MessageSending = origin.Sources.MessageSending },
				Grants = origin.Grants.Select(s => new ReputationSettingsPM.GrantRole()
					{ Level = s.Level, Role = s.Role.Id, Type = s.Type }).ToArray(),

				BanThreshold = origin.BanThreshold,
				GlobalLegalLevelIncrease = origin.GlobalLegalLevelIncrease,
				GlobalServerActivityDecrease = origin.GlobalServerActivityDecrease
			});
		}

		public async Task<ReputationSettings> ConvertUpAsync(IServer server, ReputationSettingsPM pm)
		{
			var tasks = pm.Grants.Select(async s => new ReputationSettings.GrantRole(await server.GetRoleAsync(s.Role), s.Level, s.Type)).ToArray();
			await Task.WhenAll(tasks);
			var grants = tasks.Select(s => s.Result).ToArray();

			return new ReputationSettings(grants, new ReputationSettings.Source(pm.Sources.VoiceCreation, pm.Sources.MessageSending),
				pm.BanThreshold, pm.GlobalLegalLevelIncrease, pm.GlobalServerActivityDecrease);
		}
	}
}
