global using CGZBot3.Interfaces;
global using CGZBot3.Entities;

using CGZBot3;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("settings.json").Build();

DataBaseContext.AddSettings(config.GetSection("DataBase"));