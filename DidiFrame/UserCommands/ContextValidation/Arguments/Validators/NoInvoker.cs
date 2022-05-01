namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	public class NoInvoker : AbstractArgumentValidator<IMember>
	{
		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, IMember value)
		{
			return context.Invoker == value ? new("MemberIsInvoker", UserCommandCode.InvalidInput) : null;
		}
	}
}
