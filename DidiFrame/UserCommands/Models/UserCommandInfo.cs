namespace DidiFrame.UserCommands.Models
{
	public delegate Task<UserCommandResult> UserCommandHandler(UserCommandContext ctx);


	public record UserCommandInfo(string Name, UserCommandHandler Handler, IReadOnlyList<UserCommandArgument> Arguments, IStringLocalizer Localizer, IReadOnlyCollection<IUserCommandInvokerFilter> InvokerFilters);
}
