namespace CGZBot3.Systems.Streaming
{
	public interface ISystemNotifier
	{
		public event Action<StreamAnnouncedEventArgs>? StreamAnnounced;
	}


	public record StreamAnnouncedEventArgs(StreamLifetime Lifetime, IMember Owner);
}
