using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Logging
{
	public static class LoggingExtensions
	{
		/// <summary>
		/// Adds fancy logging 
		/// </summary>
		/// <param name="builder">Logging builder from AddLogging() extension method</param>
		/// <param name="start">Date and time of bot start that will be displayed</param>
		/// <returns>Given builder to be chained</returns>
		public static ILoggingBuilder AddFacnyConsoleLogging(this ILoggingBuilder builder, DateTime start)
		{
			builder.Services.AddTransient<ILoggerProvider, FancyConsoleLoggerProvider>((services) =>
				new FancyConsoleLoggerProvider(services.GetRequiredService<Colorify.Format>(), start));
			return builder;
		}
	}
}
