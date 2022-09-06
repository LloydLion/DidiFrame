namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	/// <summary>
	/// Validator that requires "normal" string
	/// Key: WhiteSpace
	/// </summary>
	public class NormalString : IUserCommandArgumentValidator
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.NormalString
		/// </summary>
		public NormalString() { }


		/// <inheritdoc/>
		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
		{
			if (string.IsNullOrWhiteSpace(value.As<string>())) return new("WhiteSpace", UserCommandCode.InvalidInputFormat);
			else return null;
		}
	}
}
