namespace CGZBot3.UserCommands.ArgumentsValidation
{
	public interface IUserCommandArgumentValidator
	{
		public string? Validate(IServiceProvider services, UserCommandContext context, UserCommandInfo.Argument argument, object value);
	}
}
