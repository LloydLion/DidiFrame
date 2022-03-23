using CGZBot3.Data.Lifetime;
using CGZBot3.GlobalEvents;

namespace CGZBot3.Systems.Voice
{
	public class SystemCore : ISystemNotifier
	{
		private readonly IServersLifetimesRepository<CreatedVoiceChannelLifetime, CreatedVoiceChannel> repository;
		private readonly IServersSettingsRepository<VoiceSettings> settings;
		private readonly IValidator<VoiceChannelCreationArgs> creationArgsValidator;
		private readonly UIHelper uiHelper;

		public SystemCore(
			IServersLifetimesRepositoryFactory factory,
			IServersSettingsRepositoryFactory settingsFactory,
			IValidator<VoiceChannelCreationArgs> creationArgsValidator,
			UIHelper uiHelper)
		{
			repository = factory.Create<CreatedVoiceChannelLifetime, CreatedVoiceChannel>(StatesKeys.VoiceSystem);
			settings = settingsFactory.Create<VoiceSettings>(SettingsKeys.VoiceSystem);
			this.creationArgsValidator = creationArgsValidator;
			this.uiHelper = uiHelper;
		}


		public event EventHandler<VoiceChannelCreatedEventArgs>? ChannelCreated;


		public async Task<CreatedVoiceChannelLifetime> CreateAsync(VoiceChannelCreationArgs args)
		{
			creationArgsValidator.ValidateAndThrow(args);

			var t1 = CreateVoiceChannelAsync(args.Name, args.Owner);
			var t2 = CreateReportAsync(args.Name, args.Owner);

			await Task.WhenAll(t1, t2);

			var lt = repository.AddLifetime(new CreatedVoiceChannel(args.Name, t1.Result, t2.Result, args.Owner, default));

			ChannelCreated?.Invoke(this, new VoiceChannelCreatedEventArgs(lt, args));

			return lt;
		}

		public Task FixChannelLifetimeAsync(CreatedVoiceChannelLifetime lifetime)
		{
			using var baseObj = lifetime.GetBase();

			return Task.WhenAll(checkChannel(), checkReport());


			async Task checkChannel()
			{
				var bc = baseObj.Object.BaseChannel;
				if (bc.IsExist == false) baseObj.Object.BaseChannel = await CreateVoiceChannelAsync(baseObj.Object.Name, baseObj.Object.Creator);
			};
			
			async Task checkReport()
			{
				var bc = baseObj.Object.ReportMessage;
				if (bc.IsExist == false) baseObj.Object.ReportMessage = await CreateReportAsync(baseObj.Object.Name, baseObj.Object.Creator);
			};
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
