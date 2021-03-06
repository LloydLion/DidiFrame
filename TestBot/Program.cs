global using DidiFrame.Entities;
global using DidiFrame.Exceptions;
global using DidiFrame.Interfaces;
global using DidiFrame.Data;
global using FluentValidation;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Localization;
global using DidiFrame.UserCommands.Models;
global using DidiFrame.UserCommands.ContextValidation.Arguments;
global using DidiFrame.UserCommands.ContextValidation.Arguments.Validators;
global using DidiFrame.UserCommands.ContextValidation.Invoker.Filters;
global using DidiFrame.Entities.Message;
global using DidiFrame.Entities.Message.Components;
global using DidiFrame.Entities.Message.Embed;

using DidiFrame.Logging;
using DidiFrame.UserCommands;
using Microsoft.Extensions.DependencyInjection;
using DidiFrame.AutoInjecting;
using DidiFrame.Data.Json;
using DidiFrame.Culture;
using DidiFrame.GlobalEvents;
using DidiFrame.Data.Lifetime;
using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.Clients.DSharp;
using DidiFrame;
using DidiFrame.Application;
using DidiFrame.Data.MongoDB;
using DidiFrame.UserCommands.Pipeline.Building;
using DidiFrame.Data.AutoKeys;
using DidiFrame.UserCommands.Loader.EmbededCommands.Help;
using DidiFrame.Statistic;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.UserCommands.Executing;
using DidiFrame.UserCommands.Pipeline.Services;
using DidiFrame.UserCommands.ContextValidation;

var appBuilder = DiscordApplicationBuilder.Create();

appBuilder.AddJsonConfiguration("settings.json");

appBuilder.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace).AddFacnyConsoleLogging(appBuilder.GetStartupTime()));

appBuilder.AddServices((services, config) =>
{
	services
		.AddJsonDataManagement(config.GetSection("Data:Json"), false, true)
		.AddMongoDataManagement(config.GetSection("Data:Mongo"), true, false)
		.AddAutoDataRepositories()
		.AddTransient<IModelFactoryProvider, DefaultModelFactoryProvider>()
		.AddDSharpClient(config.GetSection("Discord"))
		//.AddClassicMessageUserCommandPipeline(config.GetSection("Commands:Parsing"), config.GetSection("Commands:Executing"))
		.AddSimpleUserCommandsRepository()
		.AddReflectionUserCommandsLoader()
		.AddHelpCommands()
		.AddValidatorsFromAssemblyContaining<DiscordApplicationBuilder>(includeInternalTypes: true)
		.AddSettingsBasedCultureProvider()
		.AddConfiguratedLocalization()
		.AddLifetimes()
		.AddGlobalEvents()
		.AddStateBasedStatisticTools()
		.InjectAutoDependencies(new ReflectionAutoInjector());

	services.Configure<DefaultUserCommandsExecutor.Options>(config.GetSection("Commands:Executing"));

	services.AddTransient<IUserCommandContextConverter, DefaultUserCommandContextConverter>();

	services.AddUserCommandLocalService<Disposer>();

	services.AddUserCommandPipeline(builder =>
	{
		builder
			.SetSource<UserCommandPreContext, ApplicationCommandDispatcher>(true)
			.AddMiddleware(prov => prov.GetRequiredService<IUserCommandContextConverter>())
			.AddMiddleware<UserCommandContext, ValidatedUserCommandContext, ContextValidator>(true)
			.AddMiddleware<ValidatedUserCommandContext, UserCommandResult, DefaultUserCommandsExecutor>(true)
			.Build();
	});
});


var application = appBuilder.Build();

#if !DEBUG
	printLogoAnimation(application.Services.GetRequiredService<Colorify.Format>()).Wait();
#endif

application.Connect();
application.PrepareAsync().Wait();
application.AwaitForExit().Wait();



#if !DEBUG
static async Task printLogoAnimation(Colorify.Format console)
{
	for (int i = 0; i < 40; i++)
	{
		Console.WriteLine();
	}

	await Task.Delay(300);

	console.AlignCenter("C r a z y   G a m e s   Z o n e", Colorify.Colors.txtSuccess);
	await Task.Delay(100);
	console.AlignCenter("Discord bot", Colorify.Colors.txtSuccess);
	await Task.Delay(100);
	console.AlignCenter("v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? throw new ImpossibleVariantException(), Colorify.Colors.txtSuccess);
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