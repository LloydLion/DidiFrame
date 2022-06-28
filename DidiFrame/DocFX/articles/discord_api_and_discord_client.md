# Discord api and discord client

Here you learn about discord client and thier interfaces

## What is discord client

Discord client is "window" to discord server, we can interact with discord server only using client.
These are no any direct REST packeges to discord api or middle api library calls.

## How to work with it

Discord client itself implements interface [IClient](../api/DidiFrame.Interfaces.IClient.html).
Interface describes methods and properties that works with other interfaces (like IServer, IUser) and etc.

You can see list of all "discord" interfaces  in namespace [DidiFrame.Interfaces](../api/DidiFrame.Interfaces.html)

## DSharp client

These is DSharp client that based on DSharpPlus library.
Client type is [DidiFrame.Clients.DSharp.Client](../api/DidiFrame.Clients.DSharp.Client.html) from [DidiFrame.Clients.DSharp](../download.html) package.

DSharp client more than simple adaptation of DSharpPlus library, it has got a cache and disconnect-safe methods.
Many methods of interfaces are sync and DSharp client uses caches to give result at the moment.

The client requires discord bot's token to work, you can get it in Discord development portal

## Your client

About clients creation. App builder uses a client that provided by service collection.
To use custom client simple add implementation as [IClient](../api/DidiFrame.Interfaces.IClient.html) into di container.
Don't forget remove default client.

## Fake client

We can create fake client for bot testing, simple replace all methods with gugs, but now framework don't provide something like it.