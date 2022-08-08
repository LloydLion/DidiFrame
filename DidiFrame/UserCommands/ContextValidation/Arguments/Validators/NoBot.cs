namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	/// <summary>
	/// Validator that requires non-bot member
	/// Keys: IsBot
	/// </summary>
	public class NoBot : IUserCommandArgumentValidator
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.NoBot
		/// </summary>
		public NoBot() { }


		/// <inheritdoc/>
		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
		{
			return value.As<IMember>().IsBot ? new("IsBot", UserCommandCode.InvalidInput) : null;
		}
	}
}
