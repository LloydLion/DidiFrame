using DidiFrame.UserCommands.Models;
using System.Threading.Tasks;

namespace DidiFrame.Testing.UserCommands
{
	public class NoHandler
	{
		public static Task<UserCommandResult> Handler(UserCommandContext _) => Task.FromResult(UserCommandResult.CreateEmpty(UserCommandCode.Sucssesful, "Result from empty command handler"));


		public Task<UserCommandResult> InstanceHandler(UserCommandContext _) => Task.FromResult(UserCommandResult.CreateEmpty(UserCommandCode.Sucssesful, "Result from empty command handler"));
	}
}
