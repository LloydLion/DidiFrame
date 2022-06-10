# Global events

## Introduction

Global events is ..... global events that triggers at main bot's events.

Every global event is class and can be got from di container.

Now DidiFrame has only one event - [Startup](../api/DidiFrame.GlobalEvents.StartupEvent.html)

To add all global events to di container call `AddGlobalEvents()` extension method on service collection

## StartupEvent

Class - [DidiFrame.GlobalEvents.StartupEvent](../api/DidiFrame.GlobalEvents.StartupEvent.html)

This event is raised by the application builder while preparing to launch the bot.

It has invoke method and two events: `Action Startup` and `Action<IServer> ServerStartup`.

`Startup` invokes directly, `ServerStartup` invokes multiple for each server in client.

Tip: [StartupEvent](../api/DidiFrame.GlobalEvents.StartupEvent.html) has overrided + and - operators to (un)subscribe events hadlers simpler

## Using example

```cs
public static void Main(string[] args)
{
	...

	var repository = application.Services.GetRequiredService<IServerSettingsRepositpryFactory>().Create<NotifyModel>("notify");
	application.Services.GetRequiredService<StartupEvent>().ServerStartup += async (server) =>
	{
		var model = repository.GetSettings(server);
		if (model.NotifyChannel) //Check is enabled
		{
			await model.NotifyChannel.SendMessageAsync(new("Bot is working!"));
		}
	};
	
	...
}
```