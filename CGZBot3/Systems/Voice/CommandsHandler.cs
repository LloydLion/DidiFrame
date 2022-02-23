using CGZBot3.Entities.Message;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.Loader;

namespace CGZBot3.Systems.Voice
{
	public class CommandsHandler
	{
		private readonly SystemCore core;
		private readonly IValidator<SystemCore.ChannelCreationArgs> creationArgsValidator;
		private readonly IStringLocalizer<CommandsHandler> localizer;


		public CommandsHandler(SystemCore core, IValidator<SystemCore.ChannelCreationArgs> creationArgsValidator, IStringLocalizer<CommandsHandler> localizer)
		{
			this.core = core;
			this.creationArgsValidator = creationArgsValidator;
			this.localizer = localizer;
		}


		[Command("voice")]
		public async Task<UserCommandResult> CreateChannel(UserCommandContext ctx, string name)
		{
			var args = new SystemCore.ChannelCreationArgs(name, ctx.Invoker);

			var valRes = creationArgsValidator.Validate(args);
			if (!valRes.IsValid)
				return new UserCommandResult(UserCommandCode.InvalidInput) { RespondMessage = new MessageSendModel(localizer["ChannelCreationInvalidInput", valRes.Errors[0]])};

			await core.CreateAsync(args);

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["ChannelCreated", name]) };
		}
	}
}