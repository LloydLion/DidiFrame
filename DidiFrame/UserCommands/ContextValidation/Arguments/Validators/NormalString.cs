namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	/// <summary>
	/// Validator that requires "normal" string
	/// Key: WhiteSpace
	/// </summary>
	public class NormalString : AbstractArgumentValidator<string>
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.NormalString
		/// </summary>
		public NormalString() { }


		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return new("WhiteSpace", UserCommandCode.InvalidInputFormat);

			return null;
		}
	}
}
