using DidiFrame.UserCommands.Models;
using System.Threading.Tasks;

namespace TestProject.Environment.UserCommands
{
	internal static class NoHandler
	{
		public static Task<UserCommandResult> Handler(UserCommandContext _) => Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful));
	}
}
