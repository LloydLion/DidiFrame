using CGZBot3.Entities.Message;
using CGZBot3.Systems.Test.Settings;

namespace CGZBot3.Systems.Test
{
	internal class SystemCore
	{
		private readonly ITestSettingsRepository settings;


		public SystemCore(ITestSettingsRepository settings)
		{
			this.settings = settings;
		}


		public async Task SendDisplayMessageAsync(IServer server)
		{
			var sets = settings.GetSettings(server);

			await sets.TestChannel.SendMessageAsync(new MessageSendModel(sets.SomeString));
		}
	}
}
