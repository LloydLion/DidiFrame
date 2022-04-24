namespace DidiFrame.UserCommands.ArgumentsValidation.Validators
{
	public class NormalString : AbstractArgumentValidator<string>
	{
		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return "WhiteSpace";

			return null;
		}
	}
}
