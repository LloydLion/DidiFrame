namespace CGZBot3.Systems.Voice.Lifetime
{
	internal class CreatedVoiceChannelLifetimeRepository : ICreatedVoiceChannelLifetimeRepository
	{
		private readonly IServersStatesRepository<ICollection<CreatedVoiceChannel>> repository;
		private readonly IServersSettingsRepository<VoiceSettings> settings;
		private readonly ICreatedVoiceChannelLifetimeRegistry registry;


		public CreatedVoiceChannelLifetimeRepository(
			IServersStatesRepositoryFactory repositoryFactory,
			IServersSettingsRepositoryFactory settingsFactory,
			ICreatedVoiceChannelLifetimeRegistry registry)
		{
			repository = repositoryFactory.Create<ICollection<CreatedVoiceChannel>>(StatesKeys.VoiceSystem);
			settings = settingsFactory.Create<VoiceSettings>(SettingsKeys.VoiceSystem);
			this.registry = registry;
		}


		public async Task LoadStateAsync(IServer server)
		{
			var reportChannel = settings.Get(server).ReportChannel;
			foreach (var msg in await reportChannel.GetMessagesAsync()) await msg.DeleteAsync();

			using var channels = repository.GetState(server);
			foreach (var model in channels.Object) await registry.RegisterAsync(model, EndOfLifeCallBack);
		}

		public async Task<CreatedVoiceChannelLifetime> AddChannelAsync(CreatedVoiceChannel channel)
		{
			using (var channels = repository.GetState(channel.BaseChannel.Server))
			channels.Object.Add(channel);

			return await registry.RegisterAsync(channel, EndOfLifeCallBack);
		}

		private void EndOfLifeCallBack(CreatedVoiceChannelLifetime lifetime)
		{
			var server = lifetime.BaseObject.BaseChannel.Server;
			using var channels = repository.GetState(server);
			channels.Object.Remove(lifetime.BaseObject);
		}
	}
}
 