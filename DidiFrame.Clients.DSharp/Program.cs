#if DEBUG

using Colorify.UI;
using DidiFrame.Logging;
using DidiFrame.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DidiFrame.Clients.DSharp
{
	internal static class Program
	{
		public static async Task Main(string[] args)
		{
			var loggerFactory = new LoggerFactory(new[]
			{
				new FancyConsoleLoggerProvider(new Colorify.Format(Theme.Dark), DateTime.UtcNow)
			}, new LoggerFilterOptions().AddFilter("DSharpPlus.", LogLevel.Information));

			var threading = new DefaultThreadingSystem(loggerFactory.CreateLogger<DefaultThreadingSystem>());

			using var client = new DSharpClient(Options.Create(new DSharpClient.Options() { Token = File.ReadAllText("token.ini") }), loggerFactory, threading);

			await client.ConnectAsync();

			await client.AwaitForExit();
		}
	}
}

#endif
