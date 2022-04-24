namespace DidiFrame.UserCommands.ArgumentsValidation.Validators
{
	public class StringCase : AbstractArgumentValidator<string>
	{
		private readonly bool onlyUpperNotLower;


		public StringCase(bool onlyUpperNotLower)
		{
			this.onlyUpperNotLower = onlyUpperNotLower;
		}


		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, string value)
		{
			if (onlyUpperNotLower)
			{
				if (value != value.ToUpper()) return "OnlyInUpperCase";
			}
			else
			{
				if (value != value.ToLower()) return "OnlyInLowerCase";
			}

			return null;
		}
	}
}
