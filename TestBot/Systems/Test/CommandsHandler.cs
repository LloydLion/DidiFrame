using DidiFrame.Entities.Message;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.Loader.Reflection;

namespace TestBot.Systems.Test
{
	internal class CommandsHandler : ICommandsModule
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
		
		[Command("typon")]
		public Task<UserCommandResult> Typo1(UserCommandContext _, string to)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["Greeting", to]) });
		}
		
		[Command("typom")]
		public Task<UserCommandResult> Typo2(UserCommandContext _, string to)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["Greeting", to]) });
		}
		
		[Command("typot")]
		public Task<UserCommandResult> Typo3(UserCommandContext _, string to)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["Greeting", to]) });
		}
		
		[Command("typor")]
		public Task<UserCommandResult> Typo4(UserCommandContext _, string to)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["Greeting", to]) });
		}
		
		[Command("typof")]
		public Task<UserCommandResult> Typo5(UserCommandContext _, string to)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["Greeting", to]) });
		}
		
		[Command("typoe")]
		public Task<UserCommandResult> Typo6(UserCommandContext _, string to)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["Greeting", to]) });
		}
		
		[Command("typod")]
		public Task<UserCommandResult> Typo7(UserCommandContext _, string to)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["Greeting", to]) });
		}
		
		[Command("typoc")]
		public Task<UserCommandResult> Typo8(UserCommandContext _, string to)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["Greeting", to]) });
		}
		
		[Command("typob")]
		public Task<UserCommandResult> Typo9(UserCommandContext _, string to)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel(localizer["Greeting", to]) });
		}
		
		[Command("typoa")]
		public Task<UserCommandResult> Typo0(UserCommandContext _, string to)
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
