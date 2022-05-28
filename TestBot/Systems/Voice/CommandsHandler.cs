using DidiFrame.Statistic;
using DidiFrame.UserCommands.Loader.Reflection;

namespace TestBot.Systems.Voice
{
	public class CommandsHandler : ICommandsModule
	{
		private static readonly StatisticEntry ChannelsCreated = new("channels_created");

		private readonly ISystemCore core;
		private readonly IStringLocalizer<CommandsHandler> localizer;
		private readonly IStatisticCollector statistic;


		public CommandsHandler(ISystemCore core, IStringLocalizer<CommandsHandler> localizer, IStatisticCollector statistic)
		{
			this.core = core;
			this.localizer = localizer;
			this.statistic = statistic;
		}


		[Command("voice")]
		public async Task<UserCommandResult> CreateChannel(UserCommandContext ctx, [Validator(typeof(StringCase), false)] string name)
		{
			await core.CreateAsync(name, ctx.Invoker);

			statistic.Collect((ref long data) => data++, ChannelsCreated, ctx.Channel.Server);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["ChannelCreated", name]) };
		}

		[Command("stats channels-created")]
		public UserCommandResult GetStatRecord(UserCommandContext ctx)
		{
			var count = statistic.Get(ChannelsCreated, ctx.Channel.Server);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["ChannelCreatedCount", count]) };
		}
	}
}