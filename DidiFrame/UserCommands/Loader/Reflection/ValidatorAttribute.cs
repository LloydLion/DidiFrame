namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Adds validator to command's argument
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
	public class ValidatorAttribute : Attribute
	{
		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.Reflection.ValidatorAttribute
		/// </summary>
		/// <param name="validatorType">Type of validator</param>
		/// <param name="ctorArgs">Parameters to create the validator</param>
		public ValidatorAttribute(Type validatorType, params object[] ctorArgs)
		{
			Validator = (IUserCommandArgumentValidator)(Activator.CreateInstance(validatorType, ctorArgs) ?? throw new ImpossibleVariantException());
		}


		/// <summary>
		/// Created validator
		/// </summary>
		public IUserCommandArgumentValidator Validator { get; }
	}
}
