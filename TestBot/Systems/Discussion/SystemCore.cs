using DidiFrame.Lifetimes;

namespace TestBot.Systems.Discussion
{
	internal class SystemCore
	{
		private readonly IServersSettingsRepository<DiscussionSettings> settings;
		private readonly ILifetimesRegistry<DiscussionChannelLifetime, DiscussionChannel> lifetimes;
		private readonly UIHelper uiHelper;


		public SystemCore(IServersSettingsRepository<DiscussionSettings> settings, ILifetimesRegistry<DiscussionChannelLifetime, DiscussionChannel> lifetimes, UIHelper uiHelper)
		{
			this.settings = settings;
			this.lifetimes = lifetimes;
			this.uiHelper = uiHelper;
		}


		public async Task<DiscussionChannelLifetime> CreateDiscussionAsync(string name, IChannelCategory category)
		{
			var setting = settings.Get(category.Server);
			if (setting.DisscussionCategories.Contains(category) == false)
				throw new ArgumentException("Category must be one of disscussion categories that described in server settings", nameof(category));

			var channel = (await category.CreateChannelAsync(new ChannelCreationModel(name, ChannelType.TextCompatible))).AsText();
			var message = await channel.SendMessageAsync(uiHelper.CreateAskMessageSendModel());

			var discussion = new DiscussionChannel(channel, message, Guid.NewGuid());

			var lt = lifetimes.RegistryLifetime(discussion);

			return lt;
		}
	}
}
