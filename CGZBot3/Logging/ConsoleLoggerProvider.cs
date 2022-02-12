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


		public ConsoleLoggerProvider(Format format)
		{
			this.format = format;
		}


		public ILogger CreateLogger(string categoryName)
		{
			return new ConsoleLogger(categoryName, format);
		}

		public void Dispose()
		{
			
		}
	}
}
