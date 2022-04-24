namespace DidiFrame.UserCommands.ArgumentsValidation.Validators
{
	public class NoBot : AbstractArgumentValidator<IMember>
	{
		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, IMember value)
		{
			return value.IsBot ? "IsBot" : null;
		}
	}
}
