namespace CGZBot3.UserCommands.ArgumentsValidation
{
	internal abstract class AbstractArgumentValidator<TValidation> : IUserCommandArgumentValidator
	{
		public string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, object value)
		{
			return Validate(context, argument, (TValidation)value);
		}

		protected abstract string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, TValidation value);
	}
}
