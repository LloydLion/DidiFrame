using System.Collections;

namespace CGZBot3.UserCommands.ArgumentsValidation.Validators
{
	internal class ForeachValidator : IUserCommandArgumentValidator
	{
		private readonly IUserCommandArgumentValidator validator;


		public ForeachValidator(Type validatorType, object[] creationArgs)
		{
			validator = (IUserCommandArgumentValidator)(Activator.CreateInstance(validatorType, creationArgs) ?? throw new ImpossibleVariantException());
		}

		public ForeachValidator(Type validatorType)
		{
			validator = (IUserCommandArgumentValidator)(Activator.CreateInstance(validatorType) ?? throw new ImpossibleVariantException());
		}


		public string? Validate(IServiceProvider services, UserCommandContext context, UserCommandInfo.Argument argument, object value)
		{
			var array = (IEnumerable)value;

			foreach (var item in array)
			{
				var tf = validator.Validate(services, context, argument, item);
				if (tf is not null) return tf;
			}

			return null;
		}
	}
}
