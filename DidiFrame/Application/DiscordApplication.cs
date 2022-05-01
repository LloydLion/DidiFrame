﻿using DidiFrame.Data.Lifetime;
using DidiFrame.GlobalEvents;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.Executing;
using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.Pipeline.Building;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Application
{
	internal class DiscordApplication : IDiscordApplication
	{
		private readonly EventId ClientInstantiationID = new(51, "ClientInstantiation");
		private readonly EventId ClientReadyID = new(52, "ClientReady");
		private readonly EventId DataPreloadCompliteID = new(53, "DataPreloadComplite");
		private readonly EventId StartEventFiredID = new(54, "StartEventFired");
		private readonly EventId LifetimeRegistrationDoneID = new(55, "LifetimeRegistrationDone");
		private readonly EventId UserCommandsHandlerDoneID = new(56, "UserCommandsHandlerDone");
		private readonly EventId ClientExitID = new(80, "ClientExit");
		private readonly EventId LoadingStartID = new(45, "LoadingStart");
		private readonly EventId LoaderStartID = new(46, "LoaderStart");
		private readonly EventId LoaderDoneID = new(47, "LoaderDone");
		private readonly EventId LoadingDoneID = new(48, "LoadingDone");


		private readonly IServiceProvider services;
		private readonly IClient client;
		private readonly ILogger<DiscordApplication> logger;
		private readonly DateTime now;
		private bool connected = false;


		public DiscordApplication(IServiceProvider services, DateTime now)
		{
			this.services = services;
			client = services.GetRequiredService<IClient>();
			logger = services.GetRequiredService<ILogger<DiscordApplication>>();
			logger.Log(LogLevel.Debug, ClientInstantiationID, "Client instance created");
			this.now = now;
		}


		public Task AwaitForExit()
		{
			if (connected == false)
				throw new InvalidOperationException("Please connect before await for exit (method - Connect())");

			return client.AwaitForExit().ContinueWith((_) =>
				logger.Log(LogLevel.Information, ClientExitID, "Client exited. Bot work done. Worktime - {Time}h", (DateTime.Now - now).TotalHours));
		}

		public void Connect()
		{
			client.Connect();
			connected = true;
			logger.Log(LogLevel.Information, ClientReadyID, "Client connected to discord server");
		}

		public async Task PrepareAsync()
		{
			logger.Log(LogLevel.Trace, LoadingStartID, "Commands loading is begining");
			var rp = services.GetService<IUserCommandsRepository>() ?? throw new ImpossibleVariantException();
			foreach (var loader in services.GetServices<IUserCommandsLoader>())
			{
				logger.Log(LogLevel.Trace, LoaderStartID, "Loding commands from {HandlerType}", loader.GetType().FullName);
				loader.LoadTo(rp);
				logger.Log(LogLevel.Debug, LoaderDoneID, "Loding from {HandlerType} complite", loader.GetType().FullName);
			}

			rp.Bulk();
			logger.Log(LogLevel.Information, LoadingDoneID, "Commands loding complite. Commands repository done");


			await Task.WhenAll(services.GetRequiredService<IServersStatesRepositoryFactory>().PreloadDataAsync(),
				services.GetRequiredService<IServersSettingsRepositoryFactory>().PreloadDataAsync());
			logger.Log(LogLevel.Debug, DataPreloadCompliteID, "Servers data preloaded (states and settings)");


			services.GetRequiredService<StartupEvent>().InvokeStartup();
			logger.Log(LogLevel.Debug, StartEventFiredID, "Startup event fired and finished with sucsess");


			foreach (var registry in services.GetServices<ILifetimesRegistry>())
				foreach (var server in client.Servers) registry.LoadAndRunAll(server);
			logger.Log(LogLevel.Debug, LifetimeRegistrationDoneID, "Every lifetime loaded and started");


			var pipelineBuilder = services.GetRequiredService<IUserCommandPipelineBuilder>();
			var executor = services.GetRequiredService<IUserCommandPipelineExecutor>();
			var pipeline = pipelineBuilder.Build(services);
			pipeline.Origin.SetSyncCallback((obj, sendData, callback) =>
			{
				var result = executor.Process(obj, sendData);
				if (result is not null) callback(result);
			});
			logger.Log(LogLevel.Debug, UserCommandsHandlerDoneID, "UserCommandsHandler instance created and event handler registrated");
		}


		public IServiceProvider Services => services;

		public ILogger Logger => logger;
	}
}
