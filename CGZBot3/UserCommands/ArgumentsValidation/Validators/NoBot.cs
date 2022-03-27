namespace CGZBot3.UserCommands.ArgumentsValidation.Validators
{
	internal class NoBot : AbstractArgumentValidator<IMember>
	{
		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, IMember value)
		{
			return value.IsBot ? "IsBot" : null;
		}
	}
}
