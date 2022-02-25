using CGZBot3.GlobalEvents;

namespace CGZBot3.Systems.Voice
{
	public class SystemCore
	{
		private readonly ICreatedVoiceChannelLifetimeRepository repository;
		private readonly ISettingsRepository settings;
		private readonly IValidator<ChannelCreationArgs> creationArgsValidator;


		public SystemCore(
			ICreatedVoiceChannelLifetimeRepository repository,
			ISettingsRepository settings,
			IValidator<ChannelCreationArgs> creationArgsValidator,
			StartupEvent startupEvent)
		{
			this.repository = repository;
			this.settings = settings;
			this.creationArgsValidator = creationArgsValidator;
			startupEvent.ServerStartup += ServerStartup;
		}


		private async void ServerStartup(IServer server)
		{
			await repository.LoadStateAsync(server);
		}


		public async Task<CreatedVoiceChannelLifetime> CreateAsync(ChannelCreationArgs args)
		{
			creationArgsValidator.ValidateAndThrow(args);

			var server = args.Owner.Server;

			var setting = await settings.GetSettingsAsync(server);
			var category = setting.CreationCategory;

			var channel = (await category.CreateChannelAsync(new ChannelCreationModel(args.Name, ChannelType.Voice))).AsVoice();

			var model = new CreatedVoiceChannel(args.Name, channel, args.Owner);

			return await repository.AddChannelAsync(model);
		}


		public record ChannelCreationArgs(string Name, IMember Owner);
	}
}
