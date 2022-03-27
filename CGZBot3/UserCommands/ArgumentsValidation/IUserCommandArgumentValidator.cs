namespace CGZBot3.UserCommands.ArgumentsValidation
{
	public interface IUserCommandArgumentValidator
	{
		public string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, object value);
	}
}
