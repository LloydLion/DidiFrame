global using CGZBot3.Entities;
global using CGZBot3.Exceptions;
global using CGZBot3.Interfaces;
global using CGZBot3.Data;
global using FluentValidation;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Localization;

using CGZBot3.Logging;
using CGZBot3.UserCommands;
using CGZBot3.Utils.StateMachine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CGZBot3.AutoInjecting;
using CGZBot3.Data.Json;
using CGZBot3.Culture;
using CGZBot3.GlobalEvents;
using CGZBot3.Data.Lifetime;
using CGZBot3.UserCommands.Loader.Reflection;
using CGZBot3.DSharpAdapter;
using CGZBot3;

using AutoInjector = CGZBot3.AutoInjecting.AutoInjector;


ILogger? logger = null;
IClient? client = null;
IServiceProvider? services = null;
var FatalErrorID = new EventId(1, "Fatal error");
IConfiguration? config = null;
var startupTime = DateTime.Now;


try
{
	config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("settings.json").Build();


	services = new ServiceCollection()
		.AddDataManagement<DefaultModelFactoryProvider>(config.GetSection("Data"))
		.AddDSharpClient(config.GetSection("Discord"))
		.AddDefaultUserCommandHandler(config.GetSection("Commands:Handling"))
		.AddSimpleUserCommandsRepository()
		.AddReflectionUserCommandsLoader()
		.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true)
		.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace).AddMyConsole(startupTime))
		.AddColorfy()
		.AddStateMachineUtility()
		.AddCultureMachine()
		.AddConfiguratedLocalization()
		.AddLifetimes()
		.AddGlobalEvents()
		.InjectAutoDependencies(new AutoInjector())
		.BuildServiceProvider();


#if !DEBUG
	printLogoAnimation(services.GetRequiredService<Format>()).Wait();
#endif


	logger = services.GetRequiredService<ILogger<Program>>();
}
catch (Exception ex)
{
	Console.WriteLine("FATAL ERROR while setup bot\n" + ex);
	return;
}






try
{
	var LoadingStartID = new EventId(45, "LoadingStart");
	var LoaderStartID = new EventId(46, "LoaderStart");
	var LoaderDoneID = new EventId(47, "LoaderDone");
	var LoadingDoneID = new EventId(48, "LoadingDone");

	using (logger.BeginScope("Loading commands"))
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
	}






	var ClientInstantiationID = new EventId(51, "ClientInstantiation");
	var ClientReadyID = new EventId(52, "ClientReady");
	var DataPreloadCompliteID = new EventId(53, "DataPreloadComplite");
	var StartEventFiredID = new EventId(54, "StartEventFired");
	var LifetimeRegistrationDoneID = new EventId(55, "LifetimeRegistrationDone");
	var UserCommandsHandlerDoneID = new EventId(56, "UserCommandsHandlerDone");
	var ClientExitID = new EventId(80, "ClientExit");

	using (logger.BeginScope("Creating client"))
	{
		client = services.GetService<IClient>() ?? throw new ImpossibleVariantException();
		logger.Log(LogLevel.Debug, ClientInstantiationID, "Client instance created");


		client.Connect();
		logger.Log(LogLevel.Information, ClientReadyID, "Client connected to discord server");


		Task.WaitAll(services.GetRequiredService<IServersStatesRepositoryFactory>().PreloadDataAsync(),
			services.GetRequiredService<IServersSettingsRepositoryFactory>().PreloadDataAsync());
		logger.Log(LogLevel.Debug, DataPreloadCompliteID, "Servers data preloaded (states and settings)");


		services.GetRequiredService<StartupEvent>().InvokeStartup();
		logger.Log(LogLevel.Debug, StartEventFiredID, "Startup event fired and finished with sucsess");


		foreach (var registry in services.GetServices<ILifetimesRegistry>())
			foreach (var server in client.Servers) registry.LoadAndRunAll(server);
		logger.Log(LogLevel.Debug, LifetimeRegistrationDoneID, "Every lifetime loaded and started");


		var cmdHandler = services.GetService<IUserCommandsHandler>() ?? throw new ImpossibleVariantException();
		client.CommandsDispatcher.CommandWritten += (ctx, callback) => { cmdHandler.HandleAsync(ctx, callback); };
		logger.Log(LogLevel.Debug, UserCommandsHandlerDoneID, "UserCommandsHandler instance created and event handler registrated");


		client.AwaitForExit().Wait();
		logger.Log(LogLevel.Information, ClientExitID, "Client exited. Bot work done. Worktime - {Time}h", (DateTime.Now - startupTime).TotalHours);
	}
}
catch (Exception ex)
{
	logger.Log(LogLevel.Critical, FatalErrorID, ex, "Fatal error on startup");
}





#if !DEBUG
static async Task printLogoAnimation(Format console)
{
	for (int i = 0; i < 40; i++)
	{
		Console.WriteLine();
	}

	await Task.Delay(300);

	console.AlignCenter("C r a z y   G a m e s   Z o n e", Colors.txtSuccess);
	await Task.Delay(100);
	console.AlignCenter("Discord bot", Colors.txtSuccess);
	await Task.Delay(100);
	console.AlignCenter("v" + Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? throw new ImpossibleVariantException(), Colors.txtSuccess);
	await Task.Delay(100);

	for (int i = 0; i < 15; i++)
	{
		await Task.Delay(100);
		if (i == 10) console.AlignCenter("Created by LloydLion");
		if (i == 11) console.AlignCenter("Discord server: PJMByGaaxm, Github: LloydLion");
		Console.WriteLine();
	}

	await Task.Delay(5000);

	Console.Clear();
}
#endif