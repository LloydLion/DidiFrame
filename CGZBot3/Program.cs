global using CGZBot3.Entities;
global using CGZBot3.Interfaces;
global using Microsoft.Extensions.Options;

using CGZBot3;
using CGZBot3.UserCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("settings.json").Build();

var services = new ServiceCollection()
	.Configure<DataBaseContext.Options>(config.GetSection("DataBase"))
	.AddDbContext<DataBaseContext>()
	.Configure<CGZBot3.DSharpAdapter.Client.Options>(config.GetSection("Discord"))
	.AddSingleton<IClient, CGZBot3.DSharpAdapter.Client>()
	.Configure<UserCommandsHandler.Options>(config.GetSection("Commands"))
	.AddTransient<IUserCommandsHandler, UserCommandsHandler>()
	.AddSingleton<IUserCommandsRepository, UserCommandsRepository>()
	.AddSingleton<IUserCommandsRepositoryBuilder, UserCommandsRepository.Builder>()
	.BuildServiceProvider();

var client = services.GetService<IClient>() ?? throw new Exception();

client.Connect();

client.AwaitForExit();