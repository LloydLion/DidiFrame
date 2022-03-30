using CGZBot3.Data.Lifetime;

namespace CGZBot3.Systems.Voice
{
	public class SystemCore : ISystemCore, ISystemNotifier
	{
		private readonly IServersLifetimesRepository<CreatedVoiceChannelLifetime, CreatedVoiceChannel> repository;
		private readonly IServersSettingsRepository<VoiceSettings> settings;
		private readonly UIHelper uiHelper;


		public SystemCore(
			IServersLifetimesRepositoryFactory factory,
			IServersSettingsRepositoryFactory settingsFactory,
			UIHelper uiHelper)
		{
			repository = factory.Create<CreatedVoiceChannelLifetime, CreatedVoiceChannel>(StatesKeys.VoiceSystem);
			settings = settingsFactory.Create<VoiceSettings>(SettingsKeys.VoiceSystem);
			this.uiHelper = uiHelper;
		}


		public event EventHandler<VoiceChannelCreatedEventArgs>? ChannelCreated;


		public async Task<CreatedVoiceChannelLifetime> CreateAsync(string name, IMember owner)
		{
			var t1 = CreateVoiceChannelAsync(name, owner);
			var t2 = CreateReportAsync(name, owner);

			await Task.WhenAll(t1, t2);

			var lt = repository.AddLifetime(new CreatedVoiceChannel(name, t1.Result, t2.Result, owner, default));

			ChannelCreated?.Invoke(this, new VoiceChannelCreatedEventArgs(lt, name, owner));

			return lt;
		}

		private async Task<IVoiceChannel> CreateVoiceChannelAsync(string Name, IMember owner)
		{
			var setting = settings.Get(owner.Server);
			var category = setting.CreationCategory;

			return (await category.CreateChannelAsync(new ChannelCreationModel(Name, ChannelType.Voice))).AsVoice();
		}

		private Task<IMessage> CreateReportAsync(string Name, IMember owner)
		{
			var setting = settings.Get(owner.Server);
			var report = setting.ReportChannel;

			return uiHelper.SendReportAsync(Name, owner, report);
		}
	}
}
