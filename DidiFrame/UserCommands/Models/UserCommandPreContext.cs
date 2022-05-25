namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// Raw model for command execution that will processed into DidiFrame.UserCommands.Models.UserCommandContext model
	/// </summary>
	/// <param name="Invoker">A member that has called a command</param>
	/// <param name="Channel">A channel where it has written</param>
	/// <param name="Command">A command that has been invoked</param>
	/// <param name="Arguments">Raw arguments that has been recived to command execute</param>
	public record UserCommandPreContext(
		IMember Invoker,
		ITextChannel Channel,
		UserCommandInfo Command,
		IReadOnlyDictionary<UserCommandArgument, IReadOnlyList<object>> Arguments) { }
}