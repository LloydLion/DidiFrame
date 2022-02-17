using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CGZBot3.Logging
{
	internal static class LoggingExtensions
	{
		public static ILoggingBuilder AddMyConsole(this ILoggingBuilder builder, DateTime start)
		{
			builder.Services.AddTransient<ILoggerProvider, ConsoleLoggerProvider>((services) => new ConsoleLoggerProvider(services.GetService<Colorify.Format>() ?? throw new ImpossibleVariantException(), start));
			return builder;
		}
	}
}
