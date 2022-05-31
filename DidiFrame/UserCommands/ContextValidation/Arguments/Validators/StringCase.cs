namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	/// <summary>
	/// Validator that requires specified case from string
	/// Keys: OnlyInUpperCase, OnlyInLowerCase
	/// </summary>
	public class StringCase : AbstractArgumentValidator<string>
	{
		private readonly bool onlyUpperNotLower;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.StringCase
		/// </summary>
		/// <param name="onlyUpperNotLower">If string must contains only upper case chars else only lower case chars</param>
		public StringCase(bool onlyUpperNotLower)
		{
			this.onlyUpperNotLower = onlyUpperNotLower;
		}


		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, string value)
		{
			if (onlyUpperNotLower)
			{
				if (value != value.ToUpper()) return new("OnlyInUpperCase", UserCommandCode.InvalidInputFormat);
			}
			else
			{
				if (value != value.ToLower()) return new("OnlyInLowerCase", UserCommandCode.InvalidInputFormat);
			}

			return null;
		}
	}
}
