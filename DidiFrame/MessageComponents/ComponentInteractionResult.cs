using DidiFrame.Modals;

namespace DidiFrame.Entities.Message.Components
{
	/// <summary>
	/// Model to send responce to user that called interaction
	/// </summary>
	public sealed class ComponentInteractionResult
	{
		private readonly object? parameter;
		private readonly Type type;


		private ComponentInteractionResult(Type type, object? parameter)
		{
			this.type = type;
			this.parameter = parameter;
		}


		/// <summary>
		/// Creates DidiFrame.Entities.Message.Components.ComponentInteractionResult without content
		/// </summary>
		/// <returns>New instance of DidiFrame.Entities.Message.Components.ComponentInteractionResult</returns>
		public static ComponentInteractionResult CreateEmpty() => new(Type.None, null);

		/// <summary>
		/// Creates DidiFrame.Entities.Message.Components.ComponentInteractionResult with message
		/// </summary>
		/// <param name="sendModel">Send model of message</param>
		/// <param name="subscriber">Optional subscriber to handle components</param>
		/// <returns>New instance of DidiFrame.Entities.Message.Components.ComponentInteractionResult</returns>
		public static ComponentInteractionResult CreateWithMessage(MessageSendModel sendModel, InteractionDispatcherSubscriber? subscriber = null) =>
			new(Type.Message, new MessageHelpClass(sendModel, subscriber));

		/// <summary>
		/// Creates DidiFrame.Entities.Message.Components.ComponentInteractionResult with model form
		/// </summary>
		/// <param name="modal">Modal to show</param>
		/// <returns>New instance of DidiFrame.Entities.Message.Components.ComponentInteractionResult</returns>
		public static ComponentInteractionResult CreateWithModal(IModalForm modal) => new(Type.Modal, modal);


		/// <summary>
		/// Gets modal from result if ResultType is Modal
		/// </summary>
		/// <returns>Modal to show</returns>
		/// <exception cref="InvalidOperationException">If ResultType is not Modal</exception>
		public IModalForm GetModal() =>
			type == Type.Modal ? (IModalForm)(parameter ?? throw new ImpossibleVariantException()) : throw new InvalidOperationException("Result doesn't contain modal");

		/// <summary>
		/// Gets message send model from result if ResultType is Message
		/// </summary>
		/// <returns>Message send model</returns>
		/// <exception cref="InvalidOperationException">If ResultType is not Message</exception>
		public MessageSendModel GetRespondMessage() => GetMessageHelpClass().SendModel;

		/// <summary>
		/// Gets optional subscriber from result if ResultType is Message
		/// </summary>
		/// <returns>Optional dispatcher subscriber</returns>
		/// <exception cref="InvalidOperationException">If ResultType is not Message</exception>
		public InteractionDispatcherSubscriber? GetInteractionDispatcherSubcriber() => GetMessageHelpClass().Subscriber;

		private MessageHelpClass GetMessageHelpClass() =>
			type == Type.Message ? (MessageHelpClass)(parameter ?? throw new ImpossibleVariantException()) : throw new InvalidOperationException("Result doesn't contain message");


		/// <summary>
		/// Type of result content
		/// </summary>
		public Type ResultType => type;


		/// <summary>
		/// Type of result content
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
