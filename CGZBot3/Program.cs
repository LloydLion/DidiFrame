global using CGZBot3.Entities;
global using CGZBot3.Interfaces;
global using CGZBot3.Exceptions;
global using Microsoft.Extensions.Options;
global using FluentValidation;

using CGZBot3;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CGZBot3.Logging;

var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("settings.json").Build();

IClient? client = null;

var services = new ServiceCollection()
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

	.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace).AddFilter(logFilter).AddMyConsole())

	.AddTransient(s => new Colorify.Format(Colorify.UI.Theme.Dark))

	.BuildServiceProvider();


var rp = services.GetService<IUserCommandsRepository>() ?? throw new ImpossibleVariantException();
foreach (var loader in services.GetServices<IUserCommandsLoader>()) loader.LoadTo(rp);


rp.Bulk();


client = services.GetService<IClient>() ?? throw new ImpossibleVariantException();

var cmdHandler = services.GetService<IUserCommandsHandler>() ?? throw new ImpossibleVariantException();
client.CommandsNotifier.CommandWritten += async (ctx) => { await cmdHandler.HandleAsync(ctx); };

client.Connect();
client.AwaitForExit().Wait();


bool logFilter(string provider, string category, LogLevel level)
{
	_ = client ?? throw new ImpossibleVariantException();

	if (client.IsInNamespace(category))
	{
		return level >= LogLevel.Information;
	}

	return true;
}