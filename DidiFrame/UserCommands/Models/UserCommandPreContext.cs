namespace DidiFrame.UserCommands.Models
{
    public record UserCommandPreContext(
		IMember Invoker,
		ITextChannel Channel,
		UserCommandInfo Command,
		IReadOnlyDictionary<UserCommandArgument, IReadOnlyList<object>> Arguments) { }
}