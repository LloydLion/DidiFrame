using CGZBot3.UserCommands.ArgumentsValidation;

namespace CGZBot3.UserCommands.Loader.Reflection
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
	public class ValidatorAttribute : Attribute
	{
		private readonly object validator;


		public ValidatorAttribute(Type validatorType, params object[] ctorArgs)
		{
			validator = Activator.CreateInstance(validatorType, ctorArgs) ?? throw new ImpossibleVariantException();

			if (validator is IUserCommandArgumentValidator) IsPreValidator = false;
			else if (validator is IUserCommandArgumentPreValidator) IsPreValidator = true;
			else throw new ArgumentException("Transmited type is not validator", nameof(validatorType));
		}


		public bool IsPreValidator { get; }


		public IUserCommandArgumentValidator GetAsValidator() => (IUserCommandArgumentValidator)validator;

		public IUserCommandArgumentPreValidator GetAsPreValidator() => (IUserCommandArgumentPreValidator)validator;
	}
}
