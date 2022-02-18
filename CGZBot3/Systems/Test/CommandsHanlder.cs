using CGZBot3.UserCommands;
using CGZBot3.UserCommands.Loader;

namespace CGZBot3.Systems.Test
{
	internal class CommandsHanlder
	{
		private readonly IStringLocalizer<CommandsHanlder> localizer;


		public CommandsHanlder(IStringLocalizer<CommandsHanlder> localizer)
		{
			this.localizer = localizer;
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
	}
}
