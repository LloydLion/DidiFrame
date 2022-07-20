using DidiFrame.Data.AutoKeys;
using DidiFrame.Lifetimes;

namespace TestBot.Systems.Voice
{
	public class SystemCore : ISystemCore, ISystemNotifier
	{
		private readonly ILifetimesRegistry<CreatedVoiceChannelLifetime, CreatedVoiceChannel> repository;
		private readonly IServersSettingsRepository<VoiceSettings> settings;


		public SystemCore(
			ILifetimesRegistry<CreatedVoiceChannelLifetime, CreatedVoiceChannel> repository,
			IServersSettingsRepository<VoiceSettings> settings)
		{
			this.repository = repository;
			this.settings = settings;
		}


		public event EventHandler<VoiceChannelCreatedEventArgs>? ChannelCreated;


		public async Task<CreatedVoiceChannelLifetime> CreateAsync(string name, IMember owner)
		{
			var setting = settings.Get(owner.Server);

			var channel = (await setting.CreationCategory.CreateChannelAsync(new ChannelCreationModel(name, ChannelType.Voice))).AsVoice();

			var lt = repository.RegistryLifetime(new CreatedVoiceChannel(name, channel, setting.ReportChannel, owner));

			ChannelCreated?.Invoke(this, new VoiceChannelCreatedEventArgs(lt, name, owner));

			return lt;
		}
	}
}
