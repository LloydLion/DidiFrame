using CGZBot3.Culture;
using CGZBot3.GlobalEvents;
using CGZBot3.Utils;

namespace CGZBot3.Systems.Voice
{
	public class CreatedVoiceChannelLifetimeRepository : ICreatedVoiceChannelLifetimeRepository
	{
		private readonly IClient client;
		private readonly ICreatedVoiceChannelRepository repository;
		private readonly UIHelper helper;
		private readonly ISettingsRepository settings;
		private readonly ILoggerFactory loggerFactory;
		private readonly IServerCultureProvider cultureProvider;

		private readonly List<CreatedVoiceChannelLifetime> lifetimes = new();


		public CreatedVoiceChannelLifetimeRepository(
			IClient client,
			ICreatedVoiceChannelRepository repository,
			UIHelper helper,
			ISettingsRepository settings,
			ILoggerFactory loggerFactory,
			IServerCultureProvider cultureProvider,
			StartupEvent startupEvent)
		{
			this.client = client;
			this.repository = repository;
			this.helper = helper;
			this.settings = settings;
			this.loggerFactory = loggerFactory;
			this.cultureProvider = cultureProvider;
			startupEvent.Startup += Startup;
		}


		private async void Startup()
		{
			foreach (var server in client.Servers)
			{
				cultureProvider.SetupCulture(server);

				var reportChannel = (await settings.GetSettingsAsync(server)).ReportChannel;
				foreach (var msg in await reportChannel.GetMessagesAsync()) await msg.DeleteAsync();

				await using var channels = await repository.GetChannelsAsync(server);
				lifetimes.AddRange(channels.Collection.Select(s => CreateLifetimeAsync(s).Result));
			}
		}

		private async Task<CreatedVoiceChannelLifetime> CreateLifetimeAsync(CreatedVoiceChannel model)
		{
			var logger = loggerFactory.CreateLogger<CreatedVoiceChannelLifetimeRepository>();
			var scope = logger.BeginScope("Lifetime server:{ServerId}, channel:{ChannelId}", model.BaseChannel.Server.Id, model.BaseChannel.Id);

			var report = await helper.SendReportAsync(model, (await settings.GetSettingsAsync(model.BaseChannel.Server)).ReportChannel);

			var lifetime = new CreatedVoiceChannelLifetime(model, logger, scope, report, EndOfLifeAsyncCallback);

			return lifetime;
		}

		private async Task EndOfLifeAsyncCallback(CreatedVoiceChannelLifetime lifetime)
		{
			var server = lifetime.BaseObject.BaseChannel.Server;
			lock (server) { lifetimes.Remove(lifetime); }

			await using (var channels = await repository.GetChannelsAsync(server))
			{
				channels.Collection.Remove(lifetime.BaseObject);
			}
		}

		public async Task<CreatedVoiceChannelLifetime> AddChannelAsync(CreatedVoiceChannel channel)
		{
			await using (var channels = await repository.GetChannelsAsync(channel.BaseChannel.Server))
			{
				channels.Collection.Add(channel);	
			}

			var lifetime = await CreateLifetimeAsync(channel);
			lifetimes.Add(lifetime);

			return lifetime;
		}
	}
}
 