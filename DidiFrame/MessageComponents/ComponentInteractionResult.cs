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


		public static ComponentInteractionResult CreateEmpty() => new(Type.None, null);

		public static ComponentInteractionResult CreateWithMessage(MessageSendModel sendModel, InteractionDispatcherSubscriber? subscriber = null) =>
			new(Type.Message, new MessageHelpClass(sendModel, subscriber));

		public static ComponentInteractionResult CreateWithModal(IModalForm modal) => new(Type.Modal, modal);


		public IModalForm GetModal() =>
			type == Type.Modal ? (IModalForm)(parameter ?? throw new ImpossibleVariantException()) : throw new InvalidOperationException("Result doesn't contain modal");

		public MessageSendModel GetRespondMessage() => GetMessageHelpClass().SendModel;

		public InteractionDispatcherSubscriber? GetInteractionDispatcherSubcriber() => GetMessageHelpClass().Subscriber;

		private MessageHelpClass GetMessageHelpClass() =>
			type == Type.Message ? (MessageHelpClass)(parameter ?? throw new ImpossibleVariantException()) : throw new InvalidOperationException("Result doesn't contain message");


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
