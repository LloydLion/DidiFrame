namespace CGZBot3.UserCommands.ArgumentsValidation.Validators
{
	internal class NormalString : AbstractArgumentValidator<string>
	{
		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return "WhiteSpace";

			return null;
		}
	}
}
