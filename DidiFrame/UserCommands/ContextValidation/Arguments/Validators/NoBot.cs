namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	public class NoBot : AbstractArgumentValidator<IMember>
	{
		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, IMember value)
		{
			return value.IsBot ? new("IsBot", UserCommandCode.InvalidInput) : null;
		}
	}
}
