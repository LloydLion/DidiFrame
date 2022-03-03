﻿using CGZBot3.Systems.Voice;
using System.Threading.Tasks;

namespace TestProject.SystemsTests.Voice
{
	internal class SettingsRepository : ISettingsRepository
	{
		private readonly IChannelCategory category;
		private readonly ITextChannel reportChannel;


		public SettingsRepository(IChannelCategory category, ITextChannel reportChannel)
		{
			this.category = category;
			this.reportChannel = reportChannel;
		}

		public Task<VoiceSettings> GetSettingsAsync(IServer server)
		{
			return Task.FromResult(new VoiceSettings(category, reportChannel));
		}
	}
}