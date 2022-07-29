namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// Model that contains final user command pipeline execution result
	/// </summary>
	public class UserCommandResult
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Models.UserCommandResult
		/// </summary>
		/// <param name="code">Status code</param>
		public UserCommandResult(UserCommandCode code)
		{
			Code = code;
		}


		/// <summary>
		/// Send model of messages that will be sent as respond to command
		/// </summary>
		public MessageSendModel? RespondMessage { get; init; }

		/// <summary>
		/// Debug message that will be sent to console
		/// </summary>
		public string? DebugMessage { get; init; }

		/// <summary>
		/// Exception if something went wrong
		/// </summary>
		public Exception? Exception { get; init; }

		/// <summary>
		/// Status code of command
		/// </summary>
		public UserCommandCode Code { get; }
	}
}
