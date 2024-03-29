﻿namespace DidiFrame.UserCommands.ContextValidation.Arguments
{
	/// <summary>
	/// Represents validator for command's argument
	/// </summary>
	public interface IUserCommandArgumentValidator
	{
		/// <summary>
		/// Validates argument value
		/// </summary>
		/// <param name="services">Services to be used in method</param>
		/// <param name="context">Context to validate</param>
		/// <param name="argument">Argumnet to validate</param>
		/// <param name="value">Argument value to validate</param>
		/// <param name="localServices">Local services from pipeline</param>
		/// <returns>Validation fail result if validation don't passed else null</returns>
		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices);
	}
}
