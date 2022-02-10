using CGZBot3.UserCommands;
using CGZBot3.UserCommands.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Systems.Test
{
	internal class CommandsHanlder
	{
		[Command("hello")]
		public Task<UserCommandResult> TestCmd(UserCommandContext ctx, string to)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
				{ RespondMessage = new MessageSendModel($"Hello {to} from new CGZBot3") });
		}

		[Command("sum")]
		public Task<UserCommandResult> Sum(UserCommandContext ctx, int a, int b)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
			{ RespondMessage = new MessageSendModel($"Sum result: {a + b}") });
		}

		[Command("asum")]
		public Task<UserCommandResult> Sum(UserCommandContext ctx, int[] b)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful)
			{ RespondMessage = new MessageSendModel($"Sum result: {b.Sum()}") });
		}
	}
}
