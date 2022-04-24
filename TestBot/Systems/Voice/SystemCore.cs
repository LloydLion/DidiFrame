using DidiFrame.Data.Lifetime;

namespace CGZBot3.Systems.Voice
{
	public class SystemCore : ISystemCore, ISystemNotifier
	{
		private readonly IServersLifetimesRepository<CreatedVoiceChannelLifetime, CreatedVoiceChannel> repository;
		private readonly IServersSettingsRepository<VoiceSettings> settings;


		public SystemCore(
			IServersLifetimesRepositoryFactory factory,
			IServersSettingsRepositoryFactory settingsFactory)
		{
			repository = factory.Create<CreatedVoiceChannelLifetime, CreatedVoiceChannel>(StatesKeys.VoiceSystem);
			settings = settingsFactory.Create<VoiceSettings>(SettingsKeys.VoiceSystem);
		}


		public event EventHandler<VoiceChannelCreatedEventArgs>? ChannelCreated;


		public async Task<CreatedVoiceChannelLifetime> CreateAsync(string name, IMember owner)
		{
			var setting = settings.Get(owner.Server);

			var channel = (await setting.CreationCategory.CreateChannelAsync(new ChannelCreationModel(name, ChannelType.Voice))).AsVoice();

			var lt = repository.AddLifetime(new CreatedVoiceChannel(name, channel, setting.ReportChannel, owner));

			ChannelCreated?.Invoke(this, new VoiceChannelCreatedEventArgs(lt, name, owner));

			return lt;
		}
	}
}
