﻿using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Logging
{
	public static class LoggingExtensions
	{
		public static ILoggingBuilder AddMyConsole(this ILoggingBuilder builder, DateTime start)
		{
			builder.Services.AddTransient<ILoggerProvider, ConsoleLoggerProvider>((services) =>
				new ConsoleLoggerProvider(services.GetRequiredService<Colorify.Format>(), start,
					services.GetRequiredService<ILoggingFilter>()));

			builder.Services.AddTransient<ILoggingFilter, LoggingFilter>();
			return builder;
		}
	}
}