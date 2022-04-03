namespace CGZBot3.UserCommands.ArgumentsValidation.Validators
{
	internal class NoInvoker : AbstractArgumentValidator<IMember>
	{
		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, IMember value)
		{
			return context.Invoker == value ? "MemberIsInvoker" : null;
		}
	}
}
