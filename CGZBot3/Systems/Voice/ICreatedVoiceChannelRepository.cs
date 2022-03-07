using CGZBot3.Utils;

namespace CGZBot3.Systems.Voice
{
	public interface ICreatedVoiceChannelRepository
	{
		public StateCollectionHandler<CreatedVoiceChannel> GetChannels(IServer server);
	}
}
