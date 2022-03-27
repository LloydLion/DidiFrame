using CGZBot3.Data.Lifetime;

namespace CGZBot3.Systems.Discussion
{
	internal class SystemCore
	{
		private readonly IServersSettingsRepository<DiscussionSettings> settings;
		private readonly IServersLifetimesRepository<DiscussionChannelLifetime, DiscussionChannel> lifetimes;
		private readonly UIHelper uiHelper;


		public SystemCore(IServersSettingsRepositoryFactory settings, IServersLifetimesRepositoryFactory lifetimes, UIHelper uiHelper)
		{
			this.settings = settings.Create<DiscussionSettings>(SettingsKeys.DiscussionSystem);
			this.lifetimes = lifetimes.Create<DiscussionChannelLifetime, DiscussionChannel>(StatesKeys.DiscussionSystem);
			this.uiHelper = uiHelper;
		}


		public async Task<DiscussionChannelLifetime> CreateDiscussionAsync(string name, IChannelCategory category)
		{
			var setting = settings.Get(category.Server);
			if (setting.DisscussionCategories.Contains(category) == false)
				throw new ArgumentException("Category must be one of disscussion categories that described in server settings", nameof(category));

			var channel = (await category.CreateChannelAsync(new ChannelCreationModel(name, ChannelType.TextCompatible))).AsText();
			var message = await channel.SendMessageAsync(uiHelper.CreateAskMessageSendModel());

			var discussion = new DiscussionChannel(channel, message);

			var lt = lifetimes.AddLifetime(discussion);

			return lt;
		}
	}
}
