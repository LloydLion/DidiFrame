using DidiFrame.Dependencies;
using System.Collections;

namespace DidiFrame.UserCommands.ContextValidation.Arguments.Validators
{
	/// <summary>
	/// Validator that checks every element of collection by other validator
	/// Keys will be inherited from the validator
	/// </summary>
	public class ForeachValidator : IUserCommandArgumentValidator
	{
		private readonly IUserCommandArgumentValidator validator;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.ForeachValidator
		/// </summary>
		/// <param name="validatorType">"Foreach" validator type</param>
		/// <param name="creationArgs">Parameters to create the validator</param>
		public ForeachValidator([Dependency] IServiceProvider servicesForValidator, Type validatorType, object[] creationArgs)
		{
			validator = (IUserCommandArgumentValidator)servicesForValidator.ResolveObjectWithDependencies(validatorType, creationArgs);
		}

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.ForeachValidator
		/// </summary>
		/// <param name="validatorType">"Foreach" validator type</param>
		public ForeachValidator(Type validatorType)
		{
			validator = (IUserCommandArgumentValidator)(Activator.CreateInstance(validatorType) ?? throw new ImpossibleVariantException());
		}


		/// <inheritdoc/>
		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
		{
			var array = (IEnumerable)value.ComplexObject;

			foreach (var item in array)
			{
				var tf = validator.Validate(context, new(value.Argument, item, value.PreObjects), localServices);
				if (tf is not null) return tf;
			}

			return null;
		}
	}
}
