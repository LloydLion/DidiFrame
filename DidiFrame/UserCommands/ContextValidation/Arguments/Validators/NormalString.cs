namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	public class NormalString : AbstractArgumentValidator<string>
	{
		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return new("WhiteSpace", UserCommandCode.InvalidInputFormat);

			return null;
		}
	}
}
