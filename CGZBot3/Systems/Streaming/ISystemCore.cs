namespace CGZBot3.Systems.Streaming
{
	public interface ISystemCore
	{
		public StreamLifetime AnnounceStream(string name, IMember streamer, DateTime plannedStartDate, string place);

		public StreamLifetime GetLifetime(string name, IMember streamer);
	}
}
