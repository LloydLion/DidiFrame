namespace CGZBot3.Systems.Voice
{
	public interface ICreatedVoiceChannelLifetimeRepository
	{
		public Task<CreatedVoiceChannelLifetime> AddChannelAsync(CreatedVoiceChannel channel);

		public Task LoadStateAsync(IServer server);
	}
}
