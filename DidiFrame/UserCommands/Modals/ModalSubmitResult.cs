namespace DidiFrame.UserCommands.Modals
{
	public class ModalSubmitResult
	{
		private readonly object parameter;


		private ModalSubmitResult(Type type, object parameter)
		{
			ResultType = type;
			this.parameter = parameter;
		}


		public Type ResultType { get; }


		public static ModalSubmitResult CreateSuccessful(MessageSendModel message) => new(Type.Successful, message);

		public static ModalSubmitResult CreateValidationError(ModalValidationErrorModel validationError) => new(Type.ValidationError, validationError);


		public MessageSendModel GetMessage()
		{
			if (ResultType != Type.Successful)
				throw new InvalidOperationException("Enable to get message if result type isn't Successful");
			return (MessageSendModel)parameter;
		}

		public ModalValidationErrorModel GetValidationErrorModel()
		{
			if (ResultType != Type.ValidationError)
				throw new InvalidOperationException("Enable to get validation error model if result type isn't ValidationError");
			return (ModalValidationErrorModel)parameter;
		}


		public enum Type
		{
			Successful,
			ValidationError
		}
	}
}
