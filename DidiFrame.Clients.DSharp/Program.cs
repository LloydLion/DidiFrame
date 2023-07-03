#if DEBUG
#nullable disable

using Colorify.UI;
using DidiFrame.Logging;
using DidiFrame.Threading;
using DidiFrame.Utils.RoutedEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Clients.DSharp
{
	[SuppressMessage("Major Code Smell", "S1118")]
	internal class Program
	{
		private static ILogger logger;


		public static async Task Main(string[] args)
		{
			var loggerFactory = new LoggerFactory(new[]
			{
				new FancyConsoleLoggerProvider(new Colorify.Format(Theme.Dark), DateTime.UtcNow)
			}, new LoggerFilterOptions().AddFilter("DSharpPlus.", LogLevel.Information).AddFilter("", LogLevel.Debug));

			logger = loggerFactory.CreateLogger<Program>();

			var threading = new DefaultThreadingSystem(loggerFactory.CreateLogger<DefaultThreadingSystem>());

			using var client = new DSharpClient(Options.Create(new DSharpClient.Options() { Token = File.ReadAllText("token.ini") }), loggerFactory, threading);

			client.AddListener(IServer.ServerCreated, ServerCreated);
			client.AddListener(IServer.ServerRemoved, ServerRemoved);

			client.AddListener(IServerObject.ObjectCreated, ObjectCreated);
			client.AddListener(IServerObject.ObjectModified, ObjectModified);
			client.AddListener(IServerObject.ObjectDeleted, ObjectDeleted);

			await client.ConnectAsync();

			await client.AwaitForExit();
		}


		private static void ObjectCreated(RoutedEventSender sender, IServerObject.ServerObjectEventArgs args)
		{
			logger.Log(LogLevel.Information, "Server object created event called! on {Server} with {Object}", args.Object.Server, args.Object);
		}

		private static void ObjectModified(RoutedEventSender sender, IServerObject.ServerObjectEventArgs args)
		{
			logger.Log(LogLevel.Information, "Server object modified event called! on {Server} with {Object}", args.Object.Server, args.Object);
		}

		private static void ObjectDeleted(RoutedEventSender sender, IServerObject.ServerObjectEventArgs args)
		{
			logger.Log(LogLevel.Information, "Server object deleted event called! on {Server} with {Object}", args.Object.Server, args.Object);
		}

		private static void ServerCreated(RoutedEventSender sender, IServer.ServerEventArgs args)
		{
			logger.Log(LogLevel.Information, "Server created event called! with {Server}", args.Server);
		}

		private static void ServerRemoved(RoutedEventSender sender, IServer.ServerEventArgs args)
		{
			logger.Log(LogLevel.Information, "Server removed event called! with {Server}", args.Server);
		}
	}
}

#endif
