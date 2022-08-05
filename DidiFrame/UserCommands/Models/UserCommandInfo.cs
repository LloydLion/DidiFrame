using DidiFrame.Utils.ExtendableModels;

namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// Delegate that handles user commands
	/// </summary>
	/// <param name="ctx">Context to handle</param>
	/// <returns>Task with execution result</returns>
	public delegate Task<UserCommandResult> UserCommandHandler(UserCommandContext ctx);


	/// <summary>
	/// Info about user command
	/// </summary>
	/// <param name="Name">Calling name of the command</param>
	/// <param name="Handler">Handler that will handle the command</param>
	/// <param name="Arguments">A list of arguments' infos of command</param>
	/// <param name="AdditionalInfo">A model additional info provider to provide additional and dynamic data about model</param>
	public record UserCommandInfo(string Name, UserCommandHandler Handler, IReadOnlyList<UserCommandArgument> Arguments, IModelAdditionalInfoProvider AdditionalInfo);
}
