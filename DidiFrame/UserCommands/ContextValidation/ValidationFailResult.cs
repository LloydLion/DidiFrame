namespace DidiFrame.UserCommands.ContextValidation
{
	/// <summary>
	/// Fail result of validation or filtration
	/// </summary>
	/// <param name="LocaleKey">Error locale key that will be used to get error message from CMD's localizer (cmd must support it)</param>
	/// <param name="Code">Code of command execution that will be in result of pipeline</param>
	public record ValidationFailResult(string ErrorCode, UserCommandCode Code);
}
