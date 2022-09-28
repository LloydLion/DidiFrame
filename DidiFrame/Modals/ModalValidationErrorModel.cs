namespace DidiFrame.Modals
{
	/// <summary>
	/// Model with modal validation error
	/// </summary>
	/// <param name="Message">Localized message to user</param>
	/// <param name="Component">Component that cannot be validated</param>
	public record ModalValidationErrorModel(string Message, IModalComponent Component);
}
