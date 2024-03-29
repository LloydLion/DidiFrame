﻿using DidiFrame.Culture;
using DidiFrame.Lifetimes;
using DidiFrame.GlobalEvents;
using DidiFrame.UserCommands.Loader;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.Pipeline.Building;
using Microsoft.Extensions.DependencyInjection;
using DidiFrame.UserCommands.Repository;

namespace DidiFrame.Application
{
	internal class DiscordApplication : IDiscordApplication
	{
		private readonly EventId ClientInstantiationID = new(51, "ClientInstantiation");
		private readonly EventId ClientReadyID = new(52, "ClientReady");
		private readonly EventId DataPreloadCompliteID = new(53, "DataPreloadComplite");
		private readonly EventId StartEventFiredID = new(54, "StartEventFired");
		private readonly EventId StartEventSkippedID = new(57, "StartEventSkipped");
		private readonly EventId LifetimeRegistrationDoneID = new(55, "LifetimeRegistrationDone");
		private readonly EventId UserCommandsPipelineDoneID = new(56, "UserCommandsPipelineDone");
		private readonly EventId ClientExitID = new(80, "ClientExit");
		private readonly EventId LoadingStartID = new(45, "LoadingStart");
		private readonly EventId LoaderStartID = new(46, "LoaderStart");
		private readonly EventId LoaderDoneID = new(47, "LoaderDone");
		private readonly EventId LoadingDoneID = new(48, "LoadingDone");
		private readonly EventId PipelineExecutionErrorID = new(78, "PipelineExecutionError");
		private readonly EventId ErrorMessageSendErrorID = new(77, "ErrorMessageSendError");


		private readonly IServiceProvider services;
		private readonly IClient client;
		private readonly ILogger<DiscordApplication> logger;
		private readonly IServerCultureProvider culture;
		private readonly DateTime now;
		private readonly Lazy<Task> awaitForExitTask;
		private ApplicationPhase phase = ApplicationPhase.NoConnection;


		public DiscordApplication(IServiceProvider services, DateTime now)
		{
			this.services = services;
			client = services.GetRequiredService<IClient>();
			logger = services.GetRequiredService<ILogger<DiscordApplication>>();
			culture = services.GetService<IServerCultureProvider>() ?? new GagCultureProvider(new("en-US"));

			logger.Log(LogLevel.Debug, ClientInstantiationID, "Client instance created");
			this.now = now;

			awaitForExitTask = new(AwaitForExitTaskCreator);
		}


		private Task AwaitForExitTaskCreator()
		{
			if (phase != ApplicationPhase.Ready)
				throw new InvalidOperationException("Please connect and prepare application before await for exit");

			return client.AwaitForExit().ContinueWith((_) =>
			{
				logger.Log(LogLevel.Information, ClientExitID, "Client exited. Bot work done. Worktime - {Time}h", (DateTime.Now - now).TotalHours);
				phase = ApplicationPhase.Closed;
			});
		}

		public Task AwaitForExit() => awaitForExitTask.Value;

		public async Task ConnectAsync()
		{
			if (phase != ApplicationPhase.NoConnection)
				throw new InvalidOperationException("Enable to connect twice");

			await client.ConnectAsync();
			logger.Log(LogLevel.Information, ClientReadyID, "Client connected to discord server");

			phase = ApplicationPhase.Connected;
		}

		public async Task PrepareAsync()
		{
			if (phase != ApplicationPhase.Connected)
				throw new InvalidOperationException("Please connect application before prepare it");

			logger.Log(LogLevel.Trace, LoadingStartID, "Commands loading is begining");
			var rp = services.GetService<IUserCommandsRepository>() ?? throw new ImpossibleVariantException();
			foreach (var loader in services.GetServices<IUserCommandsLoader>())
			{
				logger.Log(LogLevel.Trace, LoaderStartID, "Loding commands from {HandlerType}", loader.GetType().FullName);
				loader.LoadTo(rp);
				logger.Log(LogLevel.Debug, LoaderDoneID, "Loding from {HandlerType} complite", loader.GetType().FullName);
			}

			logger.Log(LogLevel.Information, LoadingDoneID, "Commands loding complite. Commands repository done");


			await Task.WhenAll(services.GetRequiredService<IServersStatesRepositoryFactory>().PreloadDataAsync(),
				services.GetRequiredService<IServersSettingsRepositoryFactory>().PreloadDataAsync());
			logger.Log(LogLevel.Information, DataPreloadCompliteID, "Servers data preloaded (states and settings)");


			client.SetupCultureProvider(services.GetService<IServerCultureProvider>());


			var se = services.GetService<StartupEvent>();
			if (se is not null)
			{
				se.InvokeStartup();
				logger.Log(LogLevel.Information, StartEventFiredID, "Startup event fired and finished with sucsess");
			}
			else logger.Log(LogLevel.Information, StartEventSkippedID, "Startup event skipped because hasn't been added");


			var registries = services.GetServices<ILifetimesLoader>();
			foreach (var server in client.Servers)
				foreach (var registry in registries)
				{
					culture.SetupCulture(server);
					registry.RestoreLifetimes(server);
				}
			logger.Log(LogLevel.Information, LifetimeRegistrationDoneID, "Every lifetime loaded and started");


			var pipelineBuilder = services.GetRequiredService<IUserCommandPipelineBuilder>();
			var pipeline = pipelineBuilder.Build(services);
			var executor = services.GetRequiredService<IUserCommandPipelineExecutor>();
			pipeline.Origin.SetSyncCallback(async (dispatcher, obj, sendData, state) =>
			{
				try
				{
					var result = await executor.ProcessAsync(pipeline, obj, sendData, state);
					if (result is not null) await dispatcher.RespondAsync(state, result);
					dispatcher.FinalizePipeline(state);
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Warning, PipelineExecutionErrorID, ex, "Exception catched in pipeline thread!");
					try
					{
						var msg = await sendData.Channel.SendMessageAsync(new("Command fatal!"));
						await Task.Delay(10000);
						await msg.DeleteAsync();
					}
					catch (Exception iex)
					{
						logger.Log(LogLevel.Warning, ErrorMessageSendErrorID, iex, "Enable to send or delete error message!");
					}
				}
			});
			logger.Log(LogLevel.Information, UserCommandsPipelineDoneID, "UserCommandPipeline created and event handler for executor registrated");

			phase = ApplicationPhase.Ready;
		}


		public IServiceProvider Services => services;

		public ILogger Logger => logger;


		private enum ApplicationPhase
		{
			NoConnection,
			Connected,
			Ready,
			Closed
		}
	}
}
