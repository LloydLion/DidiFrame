using System.Collections;

namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	public class ForeachValidator : IUserCommandArgumentValidator
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


		public ValidationFailResult? Validate(IServiceProvider services, UserCommandContext context, UserCommandArgument argument, UserCommandContext.ArgumentValue value)
		{
			var array = (IEnumerable)value.ComplexObject;

			foreach (var item in array)
			{
				var tf = validator.Validate(services, context, argument, new(argument, item, value.PreObjects));
				if (tf is not null) return tf;
			}

			return null;
		}
	}
}
