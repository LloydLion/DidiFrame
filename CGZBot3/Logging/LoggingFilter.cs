using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Logging
{
	internal class LoggingFilter : ILoggingFilter
	{
		private readonly IServiceProvider services;


		public LoggingFilter(IServiceProvider services)
		{
			this.services = services;
		}


		public bool Filter(string provider, string category, LogLevel level)
		{
			foreach (var option in services.GetServices<LoggingFilterOption>())
				if (level < option.GetMinLogLevel(category)) return false;
			return true;
		}
	}
}
