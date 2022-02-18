global using CGZBot3.Entities;
global using CGZBot3.Exceptions;
global using CGZBot3.Interfaces;
global using FluentValidation;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Localization;

using CGZBot3;
using CGZBot3.Logging;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.Loader;
using CGZBot3.Utils.StateMachine;
using Colorify;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using CGZBot3.SystemsInjecting;

ILogger? logger = null;
IClient? client = null;
IServiceProvider? services = null;
var FatalErrorID = new EventId(1, "Fatal error");
IConfiguration? config = null;
var startTime = DateTime.Now;

try
{
	config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("settings.json").Build();


	services = new ServiceCollection()
		.Configure<DataBaseContext.Options>(config.GetSection("DataBase"))
		.AddDbContext<DataBaseContext>()

		.Configure<CGZBot3.DSharpAdapter.Client.Options>(config.GetSection("Discord"))
		.AddSingleton<IClient, CGZBot3.DSharpAdapter.Client>()

		.Configure<UserCommandsHandler.Options>(config.GetSection("Commands:Handling"))
		.AddTransient<IUserCommandsHandler, UserCommandsHandler>()

		.AddSingleton<IUserCommandsRepository, UserCommandsRepository>()

		.Configure<ReflectionUserCommandsLoader.Options>(config.GetSection("Commands:Loading"))
		.AddTransient<IUserCommandsLoader, ReflectionUserCommandsLoader>()

		.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true)

		.AddLogging(builder => builder.AddFilter(logFilter).AddMyConsole(startTime))

		.AddTransient(s => new Colorify.Format(Colorify.UI.Theme.Dark))

		.AddTransient(typeof(IStateMachineBuilderFactory<>), typeof(StateMachineBuilderFactory<>))

		.AddLocalization(options => options.ResourcesPath = "Translations")

		.InjectAutoDependencies(new AutoInjector())

		.BuildServiceProvider();


	printLogoAnimation(services.GetRequiredService<Format>()).Wait();


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






	var ClientInstantiationID = new EventId(55, "ClientInstantiation");
	var UserCommandsHandlerDoneID = new EventId(56, "UserCommandsHandlerDone");
	var ClientReadyID = new EventId(57, "ClientReady");
	var ClientExitID = new EventId(58, "ClientExit");

	using (logger.BeginScope("Creating client"))
	{
		client = services.GetService<IClient>() ?? throw new ImpossibleVariantException();
		logger.Log(LogLevel.Debug, ClientInstantiationID, "Client instance created");


		client.Connect();
		logger.Log(LogLevel.Information, ClientReadyID, "Client connected to discord server");


		var cmdHandler = services.GetService<IUserCommandsHandler>() ?? throw new ImpossibleVariantException();
		client.CommandsNotifier.CommandWritten += (ctx) => { cmdHandler.HandleAsync(ctx); };
		logger.Log(LogLevel.Debug, UserCommandsHandlerDoneID, "UserCommandsHandler instance created and event handler registrated");


		client.AwaitForExit().Wait();
		logger.Log(LogLevel.Information, ClientExitID, "Client exited. Bot work done. Worktime - {Time}h", (DateTime.Now - startTime).TotalHours);
	}
}
catch (Exception ex)
{
	logger.Log(LogLevel.Critical, FatalErrorID, ex, "Fatal error on startup");
}






bool logModuleFilter(string provider, string category, LogLevel level)
{
	if (client is not null && client.IsInNamespace(category))
	{
		return level >= LogLevel.Information;
	}

	return true;
}

bool logLevelFilter(string provider, string category, LogLevel level)
{
	LogLevel minLogLevel;

#if DEBUG
	minLogLevel = LogLevel.Trace;
#else
	minLogLevel = config.GetSection("Logging").GetValue<LogLevel>("MinLogLevel");
#endif

	return level >= minLogLevel;
}

bool logFilter(string provider, string category, LogLevel level)
{
	return logLevelFilter(provider, category, level) && logModuleFilter(provider, category, level);
}


async Task printLogoAnimation(Format console)
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
