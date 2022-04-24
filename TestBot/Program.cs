global using DidiFrame.Entities;
global using DidiFrame.Exceptions;
global using DidiFrame.Interfaces;
global using DidiFrame.Data;
global using FluentValidation;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Localization;

using DidiFrame.Logging;
using DidiFrame.UserCommands;
using DidiFrame.Utils.StateMachine;
using Microsoft.Extensions.DependencyInjection;
using DidiFrame.AutoInjecting;
using DidiFrame.Data.Json;
using DidiFrame.Culture;
using DidiFrame.GlobalEvents;
using DidiFrame.Data.Lifetime;
using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.DSharpAdapter;
using DidiFrame;
using DidiFrame.Application;
using DidiFrame.Data.MongoDB;

using AutoInjector = DidiFrame.AutoInjecting.AutoInjector;


var appBuilder = DiscordApplicationBuilder.Create();

appBuilder.AddJsonConfiguration("settings.json");

appBuilder.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace).AddMyConsole(appBuilder.GetStartupTime()));

appBuilder.AddServices((services, config) => services
	.AddJsonDataManagement(config.GetSection("Data:Json"), false, true)
	.AddMongoDataManagement(config.GetSection("Data:Mongo"), true, false)
	.AddTransient<IModelFactoryProvider, DefaultModelFactoryProvider>()
	.AddDSharpClient(config.GetSection("Discord"))
	.AddDefaultUserCommandHandler(config.GetSection("Commands:Handling"))
	.AddSimpleUserCommandsRepository()
	.AddReflectionUserCommandsLoader()
	.AddValidatorsFromAssemblyContaining<DiscordApplicationBuilder>(includeInternalTypes: true)
	.AddColorfy()
	.AddStateMachineUtility()
	.AddCultureMachine()
	.AddConfiguratedLocalization()
	.AddLifetimes()
	.AddGlobalEvents()
	.InjectAutoDependencies(new AutoInjector()));


var application = appBuilder.Build();

#if !DEBUG
	printLogoAnimation(application.Services.GetRequiredService<Colorify.Format>()).Wait();
#endif

application.Connect();
await application.PrepareAsync();
await application.AwaitForExit();



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