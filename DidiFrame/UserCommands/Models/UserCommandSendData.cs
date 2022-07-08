namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// Model with command send data that contains a member that has called a command and a channel where it has written
	/// </summary>
	/// <param name="Invoker">A member that has called a command</param>
	/// <param name="Channel">A channel where it has written</param>
	public record UserCommandSendData(IMember Invoker, ITextChannelBase Channel);
}
