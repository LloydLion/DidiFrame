using Colorify;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Logging
{
	internal class ConsoleLoggerProvider : ILoggerProvider
	{
		private readonly Format format;
		private readonly DateTime start;


		public ConsoleLoggerProvider(Format format, DateTime start)
		{
			this.format = format;
			this.start = start;
		}


		public ILogger CreateLogger(string categoryName)
		{
			return new ConsoleLogger(categoryName, format, start);
		}

		public void Dispose()
		{
			
		}
	}
}
