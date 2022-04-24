namespace DidiFrame.Logging
{
	internal record LoggingFilterOption
	{
		private readonly Func<string, LogLevel> minLogLevel;


		public LoggingFilterOption(Func<string, LogLevel> minLogLevel)
		{
			this.minLogLevel = minLogLevel;
		}


		public LogLevel GetMinLogLevel(string category)
		{
			return minLogLevel(category);
		}
	}
}
