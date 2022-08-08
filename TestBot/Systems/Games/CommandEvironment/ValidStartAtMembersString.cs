using DidiFrame.UserCommands.ContextValidation;

namespace TestBot.Systems.Games.CommandEvironment
{
	internal class ValidStartAtMembersString : IUserCommandArgumentValidator
	{
		public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
		{
			if (value.As<string>().StartsWith("req")) return int.TryParse(value.As<string>()[3..], out _) ? null : new("FormatError", UserCommandCode.InvalidInputFormat);
			else return int.TryParse(value.As<string>(), out _) ? null : new("FormatError", UserCommandCode.InvalidInputFormat);
		}
	}
}
