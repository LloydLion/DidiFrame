using DidiFrame.Modals;

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


		/// <summary>
		/// Creates new empty instance of DidiFrame.UserCommands.Models.UserCommandResult
		/// </summary>
		/// <param name="code">Execution code</param>
		/// <param name="debugMessage">Optional debug message to log</param>
		/// <param name="exception">Optional exception to log</param>
		/// <returns>New instance of DidiFrame.UserCommands.Models.UserCommandResult</returns>
		public static UserCommandResult CreateEmpty(UserCommandCode code, string? debugMessage = null, Exception? exception = null) =>
			new(code, Type.None, null) { DebugMessage = debugMessage, Exception = exception };

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Models.UserCommandResult with message
		/// </summary>
		/// <param name="code">Execution code</param>
		/// <param name="sendModel">Send model of message</param>
		/// <param name="subscriber">Dispatcher subscriber for future message</param>
		/// <param name="debugMessage">Optional debug message to log</param>
		/// <param name="exception">Optional exception to log</param>
		/// <returns>New instance of DidiFrame.UserCommands.Models.UserCommandResult</returns>
		public static UserCommandResult CreateWithMessage(UserCommandCode code, MessageSendModel sendModel, InteractionDispatcherSubscriber? subscriber = null,
			string? debugMessage = null, Exception? exception = null) =>
			new(code, Type.Message, new MessageHelpClass(sendModel, subscriber)) { DebugMessage = debugMessage, Exception = exception };

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Models.UserCommandResult with modal
		/// </summary>
		/// <param name="code">Execution code</param>
		/// <param name="modal">Modal form object to show</param>
		/// <param name="debugMessage">Optional debug message to log</param>
		/// <param name="exception">Optional exception to log</param>
		/// <returns>New instance of DidiFrame.UserCommands.Models.UserCommandResult</returns>
		public static UserCommandResult CreateWithModal(UserCommandCode code, IModalForm modal, string? debugMessage = null, Exception? exception = null) =>
			new(code, Type.Modal, modal) { DebugMessage = debugMessage, Exception = exception };


		/// <summary>
		/// Gets send model of messages that will be sent as respond to command if result contains it
		/// </summary>
		/// <returns>Raw message send model</returns>
		/// <exception cref="InvalidOperationException">If result doesn't contain message</exception>
		public MessageSendModel GetRespondMessage() => GetMessageHelpClass().SendModel;

		/// <summary>
		/// Gets nullable interaction dispatcher subcriber if result is message
		/// </summary>
		/// <returns>Interaction dispatcher subcriber from result or null if no subscriber has been added</returns>
		/// <exception cref="InvalidOperationException">If result doesn't contain message</exception>
		public InteractionDispatcherSubscriber? GetInteractionDispatcherSubcriber() => GetMessageHelpClass().Subscriber;

		/// <summary>
		/// Gets modal form if result is modal
		/// </summary>
		/// <returns>Modal form from result</returns>
		/// <exception cref="InvalidOperationException">If result is not modal</exception>
		public IModalForm GetModal() =>
			type == Type.Modal ? (IModalForm)(parameter ?? throw new ImpossibleVariantException()) : throw new InvalidOperationException("Result doesn't contain modal");

		private MessageHelpClass GetMessageHelpClass() =>
			type == Type.Message ? (MessageHelpClass)(parameter ?? throw new ImpossibleVariantException()) : throw new InvalidOperationException("Result doesn't contain message");

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

		/// <summary>
		/// Type of result
		/// </summary>
		public Type ResultType => type;


		/// <summary>
		/// Type of result
		/// </summary>
		public enum Type
		{
			/// <summary>
			/// Empty result
			/// </summary>
			None,
			/// <summary>
			/// Result with message
			/// </summary>
			Message,
			/// <summary>
			/// Result with modal
			/// </summary>
			Modal
		}


		private sealed record MessageHelpClass(MessageSendModel SendModel, InteractionDispatcherSubscriber? Subscriber);
	}
}
