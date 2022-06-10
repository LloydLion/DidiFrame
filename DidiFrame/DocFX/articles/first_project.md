# First project

Here we create first project on DidiFrame framework step by step and write first commands.

## Step 1 - Preporations

Create new console application and download [DidiFrame nuget package](https://www.nuget.org/packages/LloydLion.DidiFrame/).
Then you need use application builder to create discord bot.
Now only one: [DiscordApplicationBuilder](../api/DidiFrame.Application.DiscordApplicationBuilder.html) exist.
Create it using `Create()` method and save into var.

Note: to create bot account visit [Discord developver portal](https://discord.com/developers/applications) and create new application.
Here you can also get token for bot.

## Step 2 - Configuration

We using the Microsoft.Extensions.Configuration library to manage configs.
Builder has method `AddConfiguration(IConfigurationRoot)` that uses to add configuration into builder, but its is not convenient.
More simple way use json configuration file. Creates new json file in project root and in properties copies this file into output directory.
Then use extension method `AddJsonConfiguration(string)` and give it path to json file relative root directory (In our case: simple file name with extension).
In json file create tipical configuration for Microsoft.Extensions.Configuration, we won't stop on it.

## Step 3 - Logging

Almost components of DidiFrame requires logging module. Logging in DidiFrame based on Microsoft.Extensions.Logging library and you can configurate it as you want.
Simple call `AddLogging(Action<ILoggingBuilder>)` method with delegate and configurate logging. But library has custom fancy logger to add it call on `ILoggingBuilder`
`AddFancyConsole(DateTime)` method with start time object that can be got from builder using `GetStartupTime()` method.

## Step 4 - Services

When configuration added we can add di container into builder. To do it call `AddServices(Action<IServiceCollection>)` method and give it configuration delegate.
In delegate we should add services into di container. DidiFrame is modules-based framework and you can enable or disable any components,
but in first time we should add Data management subsystem, Model factory provider, Auto repositories (optional), Command repository, Some command loader, Command pipeline, Localization and Discord client.
About each module further in the text.

### Data management subsystem

We sometimes need to save some data "in server" (for example reputaion point or created voice channels) or get server's configuration (for example report channel), Data management subsystem solves this problem.
From di container we can request IServersSettingsRepositoryFactory for settings and IServersStatesRepositoryFactory for states then call `Create<TModel>(string dataKey)` method to get repository that can provide state or settings for specific server.
Repositories methods are simple and intuitive and in first project guide we won't stop on it.

Tip 1: use [ConstructorAssignablePropertyAttribute](../api/DidiFrame.Data.Model.ConstructorAssignablePropertyAttribute.html) to create readonly property that will be assignable from constructor.

Tip 2: create Id property to indentitify models.

Tip 3: you can use c# records as models, but don't forget apply [ConstructorAssignablePropertyAttribute](../api/DidiFrame.Data.Model.ConstructorAssignablePropertyAttribute.html)

Framework by default has two Data management subsystems: Json based and MongoDb based, to add Json based call `AddJsonDataManagement(IConfiguration, bool, bool)` extension method on service collection,
to add MongoDb based call `AddMongoDataManagement(IConfiguration, bool, bool)` extension method. See documentation for details.

We recommend use Json for server's settings and mongo for states.

Json subsystem localated at [DidiFrame.Data.Json](../api/DidiFrame.Data.Json.html) namespace. It uses json files in special directory (set it in configuration), one file per server named as (serverId).json.

Mongo subsystem localated at [DidiFrame.Data.MongoDB](../api/DidiFrame.Data.MongoDB.html) namespace. It uses mongo database for save data. Set connection string and database name in configuration. Subsystem uses one collection per server.

### Model factory provider

Each server's state must have default value that value for a type provides [IModelFactory](../api/DidiFrame.Data.IModelFactory-1.html), but somebody must provide model factories that somebody is Model factory provider.
Default provider is [DefaultModelFactoryProvider](../api/DidiFrame.Data.DefaultModelFactoryProvider.html) (that can be added by `.AddTransient<IModelFactoryProvider, DefaultModelFactoryProvider>()` method on service collection) that takes factories from di container.
Don't forget add your models' factories into di, framework provides factory that using default model's ctor: [DefaultCtorModelFactory](../api/DidiFrame.Data.DefaultCtorModelFactory-1.html).

### Auto repositories

As stated above data repositories can be creates by data repository factories, but it isn't convenient: we have to request factory then call `Create` method and assign repository into var.
We have a solution: Auto repositories! We should mark model with [DataKeyAttribute](../api/DidiFrame.Data.AutoKeys.DataKeyAttribute.html) and request `IServers...Repository<TModel>` (without Auto repositories component, it throws ecxeption).
To add Auto repositories into di call `AddAutoDataRepositories()` extension method on service collection.

### Command repository

Here all are simple, simple store for your user commands. To add [SimpleUserCommandsRepository](../api/DidiFrame.UserCommands.Repository.SimpleUserCommandsRepository.html) use `AddSimpleUserCommandsRepository()` extension method on service collection.

Warning: [SimpleUserCommandsRepository](../api/DidiFrame.UserCommands.Repository.SimpleUserCommandsRepository.html) doesn't support per server commands.

### Command loader

Repository is cool thing, but who will fill it? Command loaders! answer we, command loader is module that loads commands on bot startup from some sources.
We will use [ReflectionUserCommandsLoader](../api/DidiFrame.UserCommands.Loader.Reflection.ReflectionUserCommandsLoader.html), this loader loads command from classes - commands modules.
It takes them from di by [ICommandsModule](../api/DidiFrame.UserCommands.Loader.Reflection.ICommandsModule.html) interface.
To add reflection loader call `AddReflectionUserCommandsLoader()` extension method on service collection.

### Command pipeline

Previous component only stores and loads commands, but somebody must execute them, for this purpose Command pipeline has been created.
Pipeline manages commands' execution process, say what command is and what can call them.
We can call `AddUserCommandPipeline(Action<IUserCommandPipelineBuilder>)` extension method and configurate pipeline manually, but framework has ready-to-use preset to add it call `AddClassicMessageUserCommandPipeline(IConfiguration, IConfiguration)` extension method on service collection.
This pipeline based on text messages and responses to a command with text.

### Discord client 

Discord client is "window" to discord server, we can interact with discord server only using client.
To add DSharpPlus-based client into di call `AddDSharpClient(IConfiguration)` extension method on service collection.

### Localization

We have to translate bot to different langs to salve this propblem created Microsoft.Extensions.Localization library that included into DidiFrame. To add it call `AddLocalization` extension method on service collection.
But we recommend to use `AddConfiguratedLocalization` extension method from DidiFrame because it also adds logging filters for localizers.

Note 1: Localization module is required by many component and without it noone module will work.

Note 2: Localization files path in DidiFrame is "DidiFrame:Translations/". Now supported 2 lang: \[en-US\] English (as default), \[ru-RU\] Russian.

## Step 5 - Run

Finnaly build application using builder's `Build()` method and call `Connect` to connect to server, `PrepareAsync()` to cache all data and prepare framework to run and `AwaitForExist()` to wait bot exit.
Now you can build and start your application, but bot won't do anything, you should add commands.

## Step 6 - Commands module

Commands module is container for commands. Create new class and write new method with a similar signature.

```cs
[Command("hello")]
public UserCommandResult SayHello(UserCommandContext ctx, string word, IMember toMention)
{
	...
}
```

Write handler in method then return [UserCommandResult](../api/DidiFrame.UserCommands.Models.UserCommandResult.html) object.
I you want use states, settings or other services request it from ctor (module is di collection element).
Commands module is done. Don't forget add it into service collection using simple `AddSingleton` or `AddTransient` methods.

## Final words

You created your first bot on DidiFrame framework!

No? You can download [example](../examples/FirstProject.zip) to research it or ask us using Github issues.

Thank you for choosing DidiFrame.