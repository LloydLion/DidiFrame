using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Logging
{
	internal static class LoggingExtensions
	{
		public static ILoggingBuilder AddMyConsole(this ILoggingBuilder builder)
		{
			builder.Services.AddTransient<ILoggerProvider, ConsoleLoggerProvider>();
			return builder;
		}
	}
}
