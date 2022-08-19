using DidiFrame.UserCommands.Modals;

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

		public static UserCommandResult CreateWithMessage(UserCommandCode code, MessageSendModel sendModel, InteractionDispatcherSubscriber? subscriber = null,
			string? debugMessage = null, Exception? exception = null) =>
			new(code, Type.Message, new MessageHelpClass(sendModel, subscriber)) { DebugMessage = debugMessage, Exception = exception };

		public static UserCommandResult CreateWithModal(UserCommandCode code, IModalForm modal, string? debugMessage = null, Exception? exception = null) =>
			new(code, Type.Modal, modal) { DebugMessage = debugMessage, Exception = exception };


		/// <summary>
		/// Gets send model of messages that will be sent as respond to command if result contains it
		/// </summary>
		/// <exception cref="InvalidOperationException">If result doesn't contain message</exception>
		public MessageSendModel GetRespondMessage() => GetMessageHelpClass().SendModel;

		public InteractionDispatcherSubscriber? GetInteractionDispatcherSubcriber() => GetMessageHelpClass().Subscriber;

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

		public Type ResultType => type;


		public enum Type
		{
			None,
			Message,
			Modal
		}


		private sealed record MessageHelpClass(MessageSendModel SendModel, InteractionDispatcherSubscriber? Subscriber);
	}
}
