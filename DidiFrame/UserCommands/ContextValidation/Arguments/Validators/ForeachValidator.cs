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
		public ForeachValidator(Type validatorType, object[] creationArgs)
		{
			validator = (IUserCommandArgumentValidator)(Activator.CreateInstance(validatorType, creationArgs) ?? throw new ImpossibleVariantException());
		}

		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.ContextValidation.Arguments.Validators.ForeachValidator
		/// </summary>
		/// <param name="validatorType">"Foreach" validator type</param>
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
