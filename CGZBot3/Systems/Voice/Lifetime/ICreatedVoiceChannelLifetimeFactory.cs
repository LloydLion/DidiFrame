namespace CGZBot3.Systems.Voice.Lifetime
{
	public interface ICreatedVoiceChannelLifetimeFactory
	{
		public Task<CreatedVoiceChannelLifetime> CreateAsync(CreatedVoiceChannel model, Action<CreatedVoiceChannelLifetime> endOfLifeCallback);
	}
}
