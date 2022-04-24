using DidiFrame.Entities.Message;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ArgumentsValidation.Validators;
using DidiFrame.UserCommands.Loader.Reflection;

namespace CGZBot3.Systems.Discussion
{
	internal class CommandsHandler : ICommandsHandler
	{
		private readonly SystemCore system;
		private readonly IStringLocalizer<CommandsHandler> localizer;


		public CommandsHandler(SystemCore system, IStringLocalizer<CommandsHandler> localizer)
		{
			this.system = system;
			this.localizer = localizer;
		}


		[Command("discuss")]
		public async Task<UserCommandResult> CreateDiscuss(UserCommandContext ctx, [Validator(typeof(StringCase), true)] string name)
		{
			await system.CreateDiscussionAsync(name, ctx.Channel.Category);			

			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["DiscussCreated", name]) };
		}
	}
}
