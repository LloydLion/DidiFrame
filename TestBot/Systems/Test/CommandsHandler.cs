using DidiFrame.Entities.Message;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.Loader.Reflection;

namespace TestBot.Systems.Test
{
	internal class CommandsHandler : ICommandsHandler
	{
		private readonly IStringLocalizer<CommandsHandler> localizer;
		private readonly SystemCore core;


		public CommandsHandler(IStringLocalizer<CommandsHandler> localizer, SystemCore core)
		{
			this.localizer = localizer;
			this.core = core;
		}


		[Command("hello")]
		public Task<UserCommandResult> TestCmd(UserCommandContext _, string to)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["Greeting", to]) });
		}

		[Command("sum")]
		public Task<UserCommandResult> Sum(UserCommandContext _, int a, int b)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["SumResult", a + b]) });
		}

		[Command("asum")]
		public Task<UserCommandResult> Sum(UserCommandContext _, int[] b)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["SumResult", b.Sum()]) });
		}

		[Command("display")]
		public async Task<UserCommandResult> Display(UserCommandContext ctx)
		{
			await core.SendDisplayMessageAsync(ctx.Invoker.Server);
			return new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new MessageSendModel(localizer["DisplayComplite"]) };
		}
	}
}
