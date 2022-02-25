namespace CGZBot3.Systems.Voice.Lifetime
{
	internal class CreatedVoiceChannelLifetimeRepository : ICreatedVoiceChannelLifetimeRepository
	{
		private readonly ICreatedVoiceChannelRepository repository;
		private readonly ISettingsRepository settings;
		private readonly ICreatedVoiceChannelLifetimeRegistry registry;


		public CreatedVoiceChannelLifetimeRepository(
			ICreatedVoiceChannelRepository repository,
			ISettingsRepository settings,
			ICreatedVoiceChannelLifetimeRegistry registry)
		{
			this.repository = repository;
			this.settings = settings;
			this.registry = registry;
		}


		public async Task LoadStateAsync(IServer server)
		{
			var reportChannel = (await settings.GetSettingsAsync(server)).ReportChannel;
			foreach (var msg in await reportChannel.GetMessagesAsync()) await msg.DeleteAsync();

			await using var channels = await repository.GetChannelsAsync(server);
			foreach (var model in channels.Collection) await registry.RegisterAsync(model, EndOfLifeCallBack);
		}

		public async Task<CreatedVoiceChannelLifetime> AddChannelAsync(CreatedVoiceChannel channel)
		{
			await using (var channels = await repository.GetChannelsAsync(channel.BaseChannel.Server))
			channels.Collection.Add(channel);

			return await registry.RegisterAsync(channel, EndOfLifeCallBack);
		}

		private async void EndOfLifeCallBack(CreatedVoiceChannelLifetime lifetime)
		{
			var server = lifetime.BaseObject.BaseChannel.Server;
			await using var channels = await repository.GetChannelsAsync(server);
			channels.Collection.Remove(lifetime.BaseObject);
		}
	}
}
 