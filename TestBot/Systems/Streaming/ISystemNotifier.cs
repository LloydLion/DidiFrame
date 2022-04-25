namespace TestBot.Systems.Streaming
{
	public interface ISystemNotifier
	{
		public event Action<StreamAnnouncedEventArgs>? StreamAnnounced;
	}


	public record StreamAnnouncedEventArgs(StreamLifetime Lifetime, IMember Owner);
}
