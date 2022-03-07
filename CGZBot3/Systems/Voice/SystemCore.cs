using CGZBot3.GlobalEvents;

namespace CGZBot3.Systems.Voice
{
	public class SystemCore : ISystemNotifier, IDisposable
	{
		private readonly ICreatedVoiceChannelLifetimeRepository repository;
		private readonly IServersSettingsRepository<VoiceSettings> settings;
		private readonly IValidator<VoiceChannelCreationArgs> creationArgsValidator;
		private readonly StartupEvent startupEvent;


		public SystemCore(
			ICreatedVoiceChannelLifetimeRepository repository,
			IServersSettingsRepositoryFactory settingsFactory,
			IValidator<VoiceChannelCreationArgs> creationArgsValidator,
			StartupEvent startupEvent)
		{
			this.repository = repository;
			settings = settingsFactory.Create<VoiceSettings>(SettingsKeys.VoiceSystem);
			this.creationArgsValidator = creationArgsValidator;
			this.startupEvent = startupEvent;
			startupEvent.ServerStartup += ServerStartup;
		}


		private async void ServerStartup(IServer server)
		{
			await repository.LoadStateAsync(server);
		}


		public event EventHandler<VoiceChannelCreatedEventArgs>? ChannelCreated;


		public async Task<CreatedVoiceChannelLifetime> CreateAsync(VoiceChannelCreationArgs args)
		{
			creationArgsValidator.ValidateAndThrow(args);

			var server = args.Owner.Server;

			var setting = settings.Get(server);
			var category = setting.CreationCategory;

			var channel = (await category.CreateChannelAsync(new ChannelCreationModel(args.Name, ChannelType.Voice))).AsVoice();

			var model = new CreatedVoiceChannel(args.Name, channel, args.Owner);

			var lt = await repository.AddChannelAsync(model);

			ChannelCreated?.Invoke(this, new VoiceChannelCreatedEventArgs(lt, args));

			return lt;
		}

		public void Dispose()
		{
			startupEvent.ServerStartup -= ServerStartup;
			GC.SuppressFinalize(this);
		}
	}
}
