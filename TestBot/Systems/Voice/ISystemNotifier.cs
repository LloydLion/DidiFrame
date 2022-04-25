namespace TestBot.Systems.Voice
{
	public interface ISystemNotifier
	{
		public event EventHandler<VoiceChannelCreatedEventArgs>? ChannelCreated;
	}


	public class VoiceChannelCreatedEventArgs : EventArgs
	{
		public VoiceChannelCreatedEventArgs(CreatedVoiceChannelLifetime lifetime, string name, IMember owner)
		{
			Lifetime = lifetime;
			Name = name;
			Owner = owner;
		}


		public CreatedVoiceChannelLifetime Lifetime { get; }

		public string Name { get; }

		public IMember Owner { get; }
	}
}
