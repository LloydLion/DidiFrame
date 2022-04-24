namespace DidiFrame.UserCommands.ArgumentsValidation.Validators
{
	public class NoInvoker : AbstractArgumentValidator<IMember>
	{
		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, IMember value)
		{
			return context.Invoker == value ? "MemberIsInvoker" : null;
		}
	}
}
