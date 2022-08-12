using DidiFrame.Culture;
using DidiFrame.Lifetimes;
using DidiFrame.GlobalEvents;
using DidiFrame.UserCommands.Loader;
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
		private bool connected = false;


		public DiscordApplication(IServiceProvider services, DateTime now)
		{
			this.services = services;
			client = services.GetRequiredService<IClient>();
			logger = services.GetRequiredService<ILogger<DiscordApplication>>();
			culture = services.GetService<IServerCultureProvider>() ?? new GagCultureProvider(new("en-US"));

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

		public async Task ConnectAsync()
		{
			await client.ConnectAsync();
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

			logger.Log(LogLevel.Information, LoadingDoneID, "Commands loding complite. Commands repository done");


			await Task.WhenAll(services.GetRequiredService<IServersStatesRepositoryFactory>().PreloadDataAsync(),
				services.GetRequiredService<IServersSettingsRepositoryFactory>().PreloadDataAsync());
			logger.Log(LogLevel.Debug, DataPreloadCompliteID, "Servers data preloaded (states and settings)");


			client.SetupCultureProvider(services.GetService<IServerCultureProvider>());


			var se = services.GetService<StartupEvent>();
			if (se is not null)
			{
				se.InvokeStartup();
				logger.Log(LogLevel.Debug, StartEventFiredID, "Startup event fired and finished with sucsess");
			}
			else logger.Log(LogLevel.Debug, StartEventSkippedID, "Startup event skipped because hasn't been added");


			var registries = services.GetServices<ILifetimesLoader>();
			foreach (var server in client.Servers)
				foreach (var registry in registries)
				{
					culture.SetupCulture(server);
					registry.RestoreLifetimes(server);
				}
			logger.Log(LogLevel.Debug, LifetimeRegistrationDoneID, "Every lifetime loaded and started");


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
						logger.Log(LogLevel.Warning, ErrorMessageSendErrorID, iex, "Enable to send error message!");
					}
				}
			});
			logger.Log(LogLevel.Debug, UserCommandsPipelineDoneID, "UserCommandPipeline created and event handler for executor registrated");
		}


		public IServiceProvider Services => services;

		public ILogger Logger => logger;
	}
}
