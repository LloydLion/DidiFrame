namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// Raw model for command execution that will processed into DidiFrame.UserCommands.Models.UserCommandContext model
	/// </summary>
	/// <param name="SendData">Send data of command</param>
	/// <param name="Command">A command that has been invoked</param>
	/// <param name="Arguments">Raw arguments that has been recived to command execute</param>
	public record UserCommandPreContext(
		UserCommandSendData SendData,
		UserCommandInfo Command,
		IReadOnlyDictionary<UserCommandArgument, IReadOnlyList<object>> Arguments) { }
}