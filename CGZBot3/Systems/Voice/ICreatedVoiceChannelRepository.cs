using CGZBot3.Utils;

namespace CGZBot3.Systems.Voice
{
	public interface ICreatedVoiceChannelRepository
	{
		public Task<StateCollectionHandler<CreatedVoiceChannel>> GetChannelsAsync(IServer server);
	}
}
