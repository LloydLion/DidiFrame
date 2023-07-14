#if DEBUG
#nullable disable

using Colorify.UI;
using DidiFrame.Clients.DSharp.Server.VSS;
using DidiFrame.Clients.DSharp.Utils;
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
		private static int magicCouter = 0;


		public static async Task Main(string[] args)
		{
			var loggerFactory = new LoggerFactory(new[]
			{
				new FancyConsoleLoggerProvider(new Colorify.Format(Theme.Dark), DateTime.UtcNow)
			}, new LoggerFilterOptions().AddFilter("DSharpPlus.", LogLevel.Information).AddFilter("", LogLevel.Debug));

			logger = loggerFactory.CreateLogger<Program>();

			var threading = new DefaultThreadingSystem(loggerFactory.CreateLogger<DefaultThreadingSystem>());

			var vssCore = new DefaultVssCore(loggerFactory.CreateLogger<EventBuffer>());

			using var client = new DSharpClient(Options.Create(new DSharpClient.Options() { Token = File.ReadAllText("token.ini") }), loggerFactory, threading, vssCore);

			client.AddListener(IServer.ServerCreatedPre, ServerCreatedPre);
			client.AddListener(IServer.ServerRemovedPre, ServerRemovedPre);

			client.AddListener(IServer.ServerCreatedPost, ServerCreatedPost);
			client.AddListener(IServer.ServerRemovedPost, ServerRemovedPost);

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
			if (args.Is<IRole>(out var role))
			{
				var file = File.OpenWrite($"vss.{magicCouter}.txt");
				var fileWriter = new StreamWriter(file);
				magicCouter++;

				fileWriter.WriteLine(DateTime.Now);
				fileWriter.WriteLine($"Changed " + role);

				foreach (var item in role.Server.ListRoles())
					fileWriter.WriteLine(item);

				file.Flush();
				fileWriter.Dispose();
			}

			logger.Log(LogLevel.Information, "Server object modified event called! on {Server} with {Object}", args.Object.Server, args.Object);
		}

		private static void ObjectDeleted(RoutedEventSender sender, IServerObject.ServerObjectEventArgs args)
		{
			logger.Log(LogLevel.Information, "Server object deleted event called! on {Server} with {Object}", args.Object.Server, args.Object);
		}

		private static void ServerCreatedPre(RoutedEventSender sender, IServer.ServerEventArgs args)
		{
			logger.Log(LogLevel.Information, "Server created pre event called! with {Server}", args.Server);
		}

		private static void ServerRemovedPre(RoutedEventSender sender, IServer.ServerEventArgs args)
		{
			logger.Log(LogLevel.Information, "Server removed pre event called! with {Server}", args.Server);
		}

		private static void ServerCreatedPost(RoutedEventSender sender, IServer.ServerEventArgs args)
		{
			logger.Log(LogLevel.Information, "Server created post event called! with {Server}", args.Server);
		}

		private static void ServerRemovedPost(RoutedEventSender sender, IServer.ServerEventArgs args)
		{
			logger.Log(LogLevel.Information, "Server removed post event called! with {Server}", args.Server);
		}
	}
}

#endif
