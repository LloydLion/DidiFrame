namespace TestBot.Systems.Streaming
{
	public interface ISystemCore
	{
		public StreamLifetime AnnounceStream(string name, IMember streamer, DateTime plannedStartDate, string place);

		public StreamLifetime GetStream(IServer server, string name);

		public bool HasStream(IServer server, string name);
	}
}
