# Statistic tools

## Description

Statistic is usefull tool to analize bot's functions and DidiFrame has methods to collect it.

Use [IStatisticCollector](../api/DidiFrame.Statistic.IStatisticCollector.html) to control it.

Tool has method `Collect(StatisticAction, StatisticEntry, IServer, long = 0)` to change current stat record and `long Get(StatisticEntry, IServer, long = 0)` to get value.

[StatisticEntry](../api/DidiFrame.Statistic.StatisticEntry.html) is information about statistic entry (record), [StatisticAction]() is delegate that accepts one *reference* paramater of long type.

## Using example

```cs
class Commands : ICommandModule
{
	private static readonly StatisticEntry HelloSayed = new("hello_sayed");
	private readonly IStatisticCollector stats;


	//Request stat tools from di
	public Commands(IStatisticCollector stats)
	{
		this.stats = stats;
	}


	[Command("hello")]
	public UserCommandResult SayHello(UserCommandContext ctx, string word, IMember toMention)
	{
		stats.Collect(s => s++, HelloSayed, ctx.Invoker.Server); //0 as default
		return new($"Hello to {toMention.Mention} with {word} from {ctx.Invoker}");
	}

	[Command("hellocount")]
	public UserCommandResult SayHello(UserCommandContext ctx)
	{
		return new($"Somebody sayed hello {stats.Get(HelloSayed, ctx.Invoker.Server)} times");
	}
}
```