namespace DidiFrame.Modals
{
	/// <summary>
	/// Result of modal submit
	/// </summary>
	public class ModalSubmitResult
	{
		private readonly object parameter;


		private ModalSubmitResult(Type type, object parameter)
		{
			ResultType = type;
			this.parameter = parameter;
		}


		/// <summary>
		/// Type of result
		/// </summary>
		public Type ResultType { get; }


		/// <summary>
		/// Creates successful submit result with message
		/// </summary>
		/// <param name="message">Responce message</param>
		/// <returns>New instance of DidiFrame.Modals.ModalSubmitResult</returns>
		public static ModalSubmitResult CreateSuccessful(MessageSendModel message) => new(Type.Successful, message);

		/// <summary>
		/// Creates errored submit result with validation error model
		/// </summary>
		/// <param name="validationError">Validation error</param>
		/// <returns>New instance of DidiFrame.Modals.ModalSubmitResult</returns>
		public static ModalSubmitResult CreateValidationError(ModalValidationErrorModel validationError) => new(Type.ValidationError, validationError);


		/// <summary>
		/// Gets message from result if ResultType is Successful
		/// </summary>
		/// <returns>Message send model</returns>
		/// <exception cref="InvalidOperationException">If ResultType is not Successful</exception>
		public MessageSendModel GetMessage()
		{
			if (ResultType != Type.Successful)
				throw new InvalidOperationException("Enable to get message if result type isn't Successful");
			return (MessageSendModel)parameter;
		}

		/// <summary>
		/// Gets validation error model from result if ResultType is ValidationError
		/// </summary>
		/// <returns>Message send model</returns>
		/// <exception cref="InvalidOperationException">If ResultType is not ValidationError</exception>
		public ModalValidationErrorModel GetValidationErrorModel()
		{
			if (ResultType != Type.ValidationError)
				throw new InvalidOperationException("Enable to get validation error model if result type isn't ValidationError");
			return (ModalValidationErrorModel)parameter;
		}


		/// <summary>
		/// Type of result
		/// </summary>
		public enum Type
		{
			/// <summary>
			/// Successful result with message
			/// </summary>
			Successful,
			/// <summary>
			/// Result with validation error
			/// </summary>
			ValidationError
		}
	}
}
