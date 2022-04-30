﻿namespace DidiFrame.UserCommands.ArgumentsValidation
{
	public interface IUserCommandArgumentValidator
	{
		public string? Validate(IServiceProvider services, UserCommandContext context, UserCommandInfo.Argument argument, UserCommandContext.ArgumentValue value);
	}
}