using CGZBot3;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("settings.json").Build();

DataBaseContext.AddSettings(config.GetSection("DataBase"));