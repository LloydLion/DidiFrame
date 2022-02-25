namespace CGZBot3.Systems.Voice.Lifetime
{
	internal interface ICreatedVoiceChannelLifetimeRegistry
	{
		public Task<CreatedVoiceChannelLifetime> RegisterAsync(CreatedVoiceChannel model, Action<CreatedVoiceChannelLifetime> endOfLifeCallback);
	}
}
