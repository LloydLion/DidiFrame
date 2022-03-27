using CGZBot3.UserCommands.ArgumentsValidation;

namespace CGZBot3.UserCommands.Loader
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
	internal class ValidatorAttribute : Attribute
	{
		public ValidatorAttribute(Type validatorType, params object[] ctorArgs)
		{
			Validator = (IUserCommandArgumentValidator)(Activator.CreateInstance(validatorType, ctorArgs) ?? throw new ImpossibleVariantException());
		}


		public IUserCommandArgumentValidator Validator { get; }
	}
}
