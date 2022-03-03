namespace CGZBot3.Systems.Voice
{
	public class VoiceChannelCreatedEventArgs : EventArgs
	{
		public VoiceChannelCreatedEventArgs(CreatedVoiceChannelLifetime lifetime, VoiceChannelCreationArgs creationArgs)
		{
			Lifetime = lifetime;
			CreationArgs = creationArgs;
		}


		public CreatedVoiceChannelLifetime Lifetime { get; }

		public VoiceChannelCreationArgs CreationArgs { get; }
	}
}
