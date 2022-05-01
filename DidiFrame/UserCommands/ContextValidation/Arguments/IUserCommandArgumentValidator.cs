namespace DidiFrame.UserCommands.ContextValidation.Arguments
{
	public interface IUserCommandArgumentValidator
	{
		public ValidationFailResult? Validate(IServiceProvider services, UserCommandContext context, UserCommandArgument argument, UserCommandContext.ArgumentValue value);
	}
}
