namespace CGZBot3.Systems.Voice
{
	public interface ISystemNotifier
	{
		public event EventHandler<VoiceChannelCreatedEventArgs>? ChannelCreated;
	}
}
