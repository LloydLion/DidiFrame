namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	/// <summary>
	/// Validator that requires specified case from string
	/// Keys: OnlyInUpperCase, OnlyInLowerCase
	/// </summary>
	public class StringCase : IUserCommandArgumentValidator
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


		/// <inheritdoc/>
		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
		{
			var strValue = value.As<string>();

			if (onlyUpperNotLower)
			{
				if (strValue != strValue.ToUpper()) return new("OnlyInUpperCase", UserCommandCode.InvalidInputFormat);
			}
			else
			{
				if (strValue != strValue.ToLower()) return new("OnlyInLowerCase", UserCommandCode.InvalidInputFormat);
			}

			return null;
		}
	}
}
