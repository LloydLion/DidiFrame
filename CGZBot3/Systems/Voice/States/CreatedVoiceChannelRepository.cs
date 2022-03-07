using CGZBot3.Utils;

namespace CGZBot3.Systems.Voice.States
{
	internal class CreatedVoiceChannelRepository : ICreatedVoiceChannelRepository
	{


		private readonly IServersStatesRepository repository;
		private readonly ThreadLocker<IServer> locker = new();


		public CreatedVoiceChannelRepository(
			IServersStatesRepository repository)
		{
			this.repository = repository;
		}


		public StateCollectionHandler<CreatedVoiceChannel> GetChannels(IServer server)
		{
			var lockFree = locker.Lock(server);

			var pmsCol = repository.GetOrCreate<ICollection<CreatedVoiceChannel>>(server, StatesKeys.VoiceSystem);
			var collection = new List<CreatedVoiceChannel>();

			return new StateCollectionHandler<CreatedVoiceChannel>(collection, (_) =>
			{
				repository.Update(server, collection, StatesKeys.VoiceSystem);
				lockFree.Dispose();
				return Task.CompletedTask;
			});
		}
	}
}
