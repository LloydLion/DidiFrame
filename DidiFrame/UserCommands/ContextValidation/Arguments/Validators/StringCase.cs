namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	public class StringCase : AbstractArgumentValidator<string>
	{
		private readonly bool onlyUpperNotLower;


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
