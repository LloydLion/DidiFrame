using DidiFrame.Entities.Message;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ArgumentsValidation.Validators;
using DidiFrame.UserCommands.Loader.Reflection;

namespace CGZBot3.Systems.Voice
{
	public class CommandsHandler : ICommandsHandler
	{
		private readonly ISystemCore core;
		private readonly IStringLocalizer<CommandsHandler> localizer;


		public CommandsHandler(ISystemCore core, IStringLocalizer<CommandsHandler> localizer)
		{
			this.core = core;
			this.localizer = localizer;
		}


		[Command("voice")]
		public async Task<UserCommandResult> CreateChannel(UserCommandContext ctx, [Validator(typeof(StringCase), false)] string name)
		{
			await core.CreateAsync(name, ctx.Invoker);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["ChannelCreated", name]) };
		}
	}
}