using DidiFrame.Utils.ExtendableModels;

namespace DidiFrame.UserCommands.Models
{
	public delegate Task<UserCommandResult> UserCommandHandler(UserCommandContext ctx);


	public record UserCommandInfo(string Name, UserCommandHandler Handler, IReadOnlyList<UserCommandArgument> Arguments, IModelAdditionalInfoProvider AdditionalInfo);
}
