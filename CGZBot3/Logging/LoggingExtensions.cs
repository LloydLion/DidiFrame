using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Logging
{
	internal static class LoggingExtensions
	{
		public static ILoggingBuilder AddMyConsole(this ILoggingBuilder builder, DateTime start)
		{
			builder.Services.AddTransient<ILoggerProvider, ConsoleLoggerProvider>((services) => new ConsoleLoggerProvider(services.GetRequiredService<Colorify.Format>(), start, services.GetRequiredService<ILoggingFilter>()));
			return builder;
		}
	}
}
