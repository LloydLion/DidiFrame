using CGZBot3.Utils;

namespace CGZBot3.Systems.Voice.States
{
	internal class CreatedVoiceChannelRepository : ICreatedVoiceChannelRepository
	{
		private static readonly EventId ObjectLoadErrorID = new(34, "ObjectLoadError");


		private readonly IModelConverter<CreatedVoiceChannelPM, CreatedVoiceChannel> converter;
		private readonly IServersStatesRepository repository;
		private readonly ILogger<CreatedVoiceChannelRepository> logger;
		private readonly ThreadLocker<IServer> locker = new();


		public CreatedVoiceChannelRepository(
			IModelConverter<CreatedVoiceChannelPM, CreatedVoiceChannel> converter,
			IServersStatesRepository repository,
			ILogger<CreatedVoiceChannelRepository> logger)
		{
			this.converter = converter;
			this.repository = repository;
			this.logger = logger;
		}


		public async Task<StateCollectionHandler<CreatedVoiceChannel>> GetChannelsAsync(IServer server)
		{
			var lockFree = locker.Lock(server);

			var pmsCol = repository.GetOrCreate<ICollection<CreatedVoiceChannelPM>>(server, StatesKeys.VoiceSystem);
			var collection = new List<CreatedVoiceChannel>();

			foreach (var pm in pmsCol)
			{
				try
				{
					collection.Add(await converter.ConvertUpAsync(server, pm));
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Warning, ObjectLoadErrorID, ex, "Can't load some CreatedVoiceChannel(server:{ServerId}, channel:{ChannelId}) object. Skip it",
						server.Id, pm.BaseChannel);
				}
			}

			return new StateCollectionHandler<CreatedVoiceChannel>(collection, async (_) =>
			{
				var pms = collection.Select(async s => await converter.ConvertDownAsync(server, s));
				await Task.WhenAll(pms);
				var collectioToSave = pms.Select(s => s.Result).ToArray();
				repository.Update(server, collectioToSave, StatesKeys.VoiceSystem);
				lockFree.Dispose();
			});
		}
	}
}
