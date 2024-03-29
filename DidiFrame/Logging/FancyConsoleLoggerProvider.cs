﻿using Colorify;

namespace DidiFrame.Logging
{
	/// <summary>
	/// Provider for fancy console logger
	/// </summary>
	public sealed class FancyConsoleLoggerProvider : ILoggerProvider
	{
		private readonly Format format;
		private readonly DateTime start;


		/// <summary>
		/// Creates new instance of DidiFrame.Logging.FancyConsoleLoggerProvider using Colorify library class instance and date and time of bot start
		/// </summary>
		/// <param name="format"></param>
		/// <param name="start"></param>
		public FancyConsoleLoggerProvider(Format format, DateTime start)
		{
			this.format = format;
			this.start = start;

			format.WriteLine($"Startup - now: {start}", Colors.txtInfo);
		}


		/// <inheritdoc/>
		public ILogger CreateLogger(string categoryName)
		{
			return new FancyConsoleLogger(categoryName, format, DateOnly.FromDateTime(start));
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}
