using Colorify;

namespace CGZBot3.Logging
{
	internal class ConsoleLoggerProvider : ILoggerProvider
	{
		private readonly Format format;
		private readonly DateTime start;
		private readonly ILoggingFilter filter;


		public ConsoleLoggerProvider(Format format, DateTime start, ILoggingFilter filter)
		{
			this.format = format;
			this.start = start;
			this.filter = filter;
		}


		public ILogger CreateLogger(string categoryName)
		{
			return new ConsoleLogger(categoryName, format, start, filter);
		}

		public void Dispose()
		{
			
		}
	}
}
