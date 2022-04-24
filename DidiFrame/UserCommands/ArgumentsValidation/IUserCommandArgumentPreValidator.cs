namespace DidiFrame.UserCommands.ArgumentsValidation
{
	public interface IUserCommandArgumentPreValidator
	{
		public string? Validate(IServiceProvider services, UserCommandPreContext context, UserCommandInfo.Argument argument, IReadOnlyList<object> values);
	}
}
