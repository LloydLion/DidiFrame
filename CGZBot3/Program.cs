global using CGZBot3.Interfaces;
global using CGZBot3.Entities;
global using Microsoft.Extensions.Options;

using CGZBot3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("settings.json").Build();

var services = new ServiceCollection()
	.Configure<DataBaseContext.Options>(config.GetSection("DataBase"))
	.AddDbContext<DataBaseContext>()
	.Configure<CGZBot3.DSharpAdapter.Client.Options>(config.GetSection("Discord"))
	.AddSingleton<IClient, CGZBot3.DSharpAdapter.Client>()
	.BuildServiceProvider();

var client = services.GetService<IClient>() ?? throw new Exception();

client.Connect();

client.AwaitForExit();