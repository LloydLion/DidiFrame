namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	/// <summary>
	/// Validator that requires non-bot member
	/// Keys: IsBot
	/// </summary>
	public class NoBot : AbstractArgumentValidator<IMember>
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.NoBot
		/// </summary>
		public NoBot() { }


		/// <inheritdoc/>
		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, IMember value)
		{
			return value.IsBot ? new("IsBot", UserCommandCode.InvalidInput) : null;
		}
	}
}
