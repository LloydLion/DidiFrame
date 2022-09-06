namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	/// <summary>
	/// Validator that requires non-invoker member
	/// Keys: MemberIsInvoker
	/// </summary>
	public class NoInvoker : IUserCommandArgumentValidator
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.NoInvoker
		/// </summary>
		public NoInvoker() { }


		/// <inheritdoc/>
		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
		{
			return context.SendData.Invoker.Equals(value.As<IMember>()) ? new("MemberIsInvoker", UserCommandCode.InvalidInput) : null;
		}
	}
}
