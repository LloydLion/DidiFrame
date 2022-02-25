namespace CGZBot3.Systems.Voice.Lifetime
{
	internal class CreatedVoiceChannelLifetimeRegistry : ICreatedVoiceChannelLifetimeRegistry
	{
		private readonly Dictionary<CreatedVoiceChannelLifetime, Action<CreatedVoiceChannelLifetime>> lifetimesCallbacks = new();
		private readonly ICreatedVoiceChannelLifetimeFactory factory;


		public CreatedVoiceChannelLifetimeRegistry(ICreatedVoiceChannelLifetimeFactory factory)
		{
			this.factory = factory;
		}


		public async Task<CreatedVoiceChannelLifetime> RegisterAsync(CreatedVoiceChannel model, Action<CreatedVoiceChannelLifetime> endOfLifeCallback)
		{
			var lifetime = await factory.CreateAsync(model, EndOfLifeCallback);
			lifetimesCallbacks.Add(lifetime, endOfLifeCallback);
			return lifetime;
		}

		private void EndOfLifeCallback(CreatedVoiceChannelLifetime lifetime)
		{
			var callback = lifetimesCallbacks[lifetime];
			lifetimesCallbacks.Remove(lifetime);

			callback(lifetime);
		}
	}
}
