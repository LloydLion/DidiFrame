using DidiFrame.UserCommands.Models;
using System.Threading.Tasks;

namespace DidiFrame.Testing.UserCommands
{
	/// <summary>
	/// Provides null handler for user commands
	/// </summary>
	public class NoHandler
	{
		/// <summary>
		/// Null handler for user commands
		/// </summary>
		/// <param name="_">Some UserCommandContext</param>
		/// <returns>Task with empty UserCommandResult</returns>
		public static Task<UserCommandResult> Handler(UserCommandContext _) => Task.FromResult(UserCommandResult.CreateEmpty(UserCommandCode.Sucssesful, "Result from empty command handler"));


		/// <summary>
		/// Null handler for user commands
		/// </summary>
		/// <param name="_">Some UserCommandContext</param>
		/// <returns>Task with empty UserCommandResult</returns>
		public Task<UserCommandResult> InstanceHandler(UserCommandContext _) => Task.FromResult(UserCommandResult.CreateEmpty(UserCommandCode.Sucssesful, "Result from empty command handler"));
	}
}
