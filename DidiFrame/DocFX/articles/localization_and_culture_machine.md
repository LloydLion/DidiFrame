# Localization and culture machine

## Localization

We have to translate bot to different langs to salve this propblem created Microsoft.Extensions.Localization library that included into DidiFrame. To add it call `AddLocalization` extension method on service collection.
But we recommend to use `AddConfiguratedLocalization` extension method from DidiFrame because it also adds logging filters for localizers.

Note 1: Localization module is required by many component and without it noone module will work.

Note 2: Localization files path in DidiFrame is "DidiFrame:Translations/". Now supported 2 lang: \[en-US\] English (as default), \[ru-RU\] Russian.

## Culture machine

Alt name is `Culture provider`.

Localization module provides text by key and current culture, but for different server we must use different server, unless localization is useless.

Note 1: Default localizers from Microsoft.Extensions.Localization library gets culture from Thread.CurrentUICulture.

Note 2: By default if no culture mahcine everywhere is en-US culture will be setuped.

Culture mahcine is source of culture info for each server. It implements [IServerCultureProvider](../api/DidiFrame.Culture.IServerCultureProvider.html).

Default implementation from DidiFrame is [SettingsBasedCultureProvider](../api/DidiFrame.Culture.SettingsBasedCultureProvider.html) that takes culture from servers' settings by `culture` key and using [CultureSettings](../api/DidiFrame.Culture.CultureSettings.html) model. To add it into di use `AddSettingsBasedCultureProvider()` extension method on service collection.

## Using example

```cs
class Commands : ICommandModule
{
	private readonly IStringLocalizer<Commands> localizer;
	private readonly IServerCultureProvider provider;


	//Request localizer and culture provider from di
	public Commands(IStringLocalizer<Commands> localizer, IServerCultureProvider provider)
	{
		this.localizer = localizer;
		this.provider = provider;
	}


	[Command("hello")]
	public UserCommandResult SayHello(UserCommandContext ctx, string word, IMember toMention)
	{
		//Thread.CurrentUICulture is automatically setted by executor
		return new(localizer["Hello", toMention.Mention, word, ctx.Invoker]);
	}
	
	[Command("culture")]
	public UserCommandResult SayHello(UserCommandContext ctx)
	{
		var culture = provider.GetCulture(ctx.Channel.Server);
		return new(localizer["Culture", culture.ToString()]);
	}
}
```