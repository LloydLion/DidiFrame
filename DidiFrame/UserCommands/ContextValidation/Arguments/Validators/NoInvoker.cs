namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	/// <summary>
	/// Validator that requires non-invoker member
	/// Keys: MemberIsInvoker
	/// </summary>
	public class NoInvoker : AbstractArgumentValidator<IMember>
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.NoInvoker
		/// </summary>
		public NoInvoker() { }


		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, IMember value)
		{
			return context.Invoker == value ? new("MemberIsInvoker", UserCommandCode.InvalidInput) : null;
		}
	}
}
