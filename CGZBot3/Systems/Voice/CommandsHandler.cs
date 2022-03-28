using CGZBot3.Entities.Message;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation.Validators;
using CGZBot3.UserCommands.Loader.Reflection;

namespace CGZBot3.Systems.Voice
{
	public class CommandsHandler : ICommandsHandler
	{
		private readonly SystemCore core;
		private readonly IStringLocalizer<CommandsHandler> localizer;


		public CommandsHandler(SystemCore core, IStringLocalizer<CommandsHandler> localizer)
		{
			this.core = core;
			this.localizer = localizer;
		}


		[Command("voice")]
		public async Task<UserCommandResult> CreateChannel(UserCommandContext ctx, [Validator(typeof(StringCase), false)] string name)
		{
			await core.CreateAsync(new VoiceChannelCreationArgs(name, ctx.Invoker));

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["ChannelCreated", name]) };
		}
	}
}