using DidiFrame.Dependencies;

namespace DidiFrame.UserCommands.Loader.Reflection
{
	/// <summary>
	/// Adds validator to command's argument
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
	public class ValidatorAttribute : Attribute
	{
		private readonly Type validatorType;
		private readonly object[] ctorArgs;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Loader.Reflection.ValidatorAttribute
		/// </summary>
		/// <param name="validatorType">Type of validator</param>
		/// <param name="ctorArgs">Parameters to create the validator</param>
		public ValidatorAttribute(Type validatorType, params object[] ctorArgs)
		{
			this.validatorType = validatorType;
			this.ctorArgs = ctorArgs;
		}


		/// <summary>
		/// Creates argument validator instance using dependencies from services
		/// </summary>
		/// <param name="services">Services to provide dependencies for instance</param>
		/// <returns>New argument validator</returns>
		public IUserCommandArgumentValidator CreateFilter(IServiceProvider services) =>
			(IUserCommandArgumentValidator)services.ResolveObjectWithDependencies(validatorType, ctorArgs);
	}
}
