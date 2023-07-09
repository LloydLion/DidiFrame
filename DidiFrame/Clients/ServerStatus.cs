namespace DidiFrame.Clients
{
	public enum ServerStatus
	{
		Created = 0,
		Startup = 1,
		Working = 2,
		PerformTermination = 3,
		Terminating = 4,
		Stopped = 5
	}


	public static class ServerStatusMethods
	{
		public static bool IsAfter(this ServerStatus toCompareWith, ServerStatus compareBase, bool inclusive = true)
		{
			return toCompareWith > compareBase || (inclusive && toCompareWith == compareBase);
		}

		public static bool IsBefore(this ServerStatus toCompareWith, ServerStatus compareBase, bool inclusive = true)
		{
			return toCompareWith < compareBase || (inclusive && toCompareWith == compareBase);
		}
	}
}
