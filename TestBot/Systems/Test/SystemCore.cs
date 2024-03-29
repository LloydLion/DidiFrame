﻿namespace TestBot.Systems.Test
{
	internal class SystemCore
	{
		private readonly IServersSettingsRepository<TestSettings> settings;


		public SystemCore(IServersSettingsRepositoryFactory settingsFactory)
		{
			settings = settingsFactory.Create<TestSettings>(SettingsKeys.TestSystem);
		}


		public async Task SendDisplayMessageAsync(IServer server)
		{
			var sets = settings.Get(server);

			await sets.TestChannel.SendMessageAsync(new MessageSendModel(sets.SomeString));
		}
	}
}
