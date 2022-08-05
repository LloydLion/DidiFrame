namespace DidiFrame.UserCommands.Models
{
	/// <summary>
	/// Model that contains final user command pipeline execution result
	/// </summary>
	public class UserCommandResult
	{
		private readonly object? parameter;
		private readonly Type type;


		private UserCommandResult(UserCommandCode code, Type type, object? parameter)
		{
			Code = code;
			this.type = type;
			this.parameter = parameter;
		}


		public static UserCommandResult CreateEmpty(UserCommandCode code, string? debugMessage = null, Exception? exception = null) =>
			new(code, Type.None, null) { DebugMessage = debugMessage, Exception = exception };

		public static UserCommandResult CreateWithMessage(UserCommandCode code, MessageSendModel sendModel, string? debugMessage = null, Exception? exception = null) =>
			new(code, Type.Message, sendModel) { DebugMessage = debugMessage, Exception = exception };


		/// <summary>
		/// Gets send model of messages that will be sent as respond to command if result contains it
		/// </summary>
		/// <exception cref="InvalidOperationException">If result doesn't contain message</exception>
		public MessageSendModel GetRespondMessage() =>
			type == Type.Message ? (MessageSendModel)(parameter ?? throw new ImpossibleVariantException()) : throw new InvalidOperationException("Result doesn't contain message");

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

		public Type ResultType => type;


		public enum Type
		{
			None,
			Message
		}
	}
}
