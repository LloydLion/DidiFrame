global using CGZBot3.Entities;
global using CGZBot3.Interfaces;
global using Microsoft.Extensions.Options;

using CGZBot3;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("settings.json").Build();

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
	.BuildServiceProvider();


var rp = services.GetService<IUserCommandsRepository>() ?? throw new Exception();
foreach (var loader in services.GetServices<IUserCommandsLoader>()) loader.LoadTo(rp);


var client = services.GetService<IClient>() ?? throw new Exception();

var cmdHandler = services.GetService<IUserCommandsHandler>() ?? throw new Exception();
client.CommandsNotifier.CommandWritten += async (ctx) => { await cmdHandler.HandleAsync(ctx); };

client.Connect();
client.AwaitForExit().Wait();