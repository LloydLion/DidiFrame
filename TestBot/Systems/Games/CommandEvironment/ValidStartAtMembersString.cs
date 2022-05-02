using DidiFrame.UserCommands.ContextValidation;

namespace TestBot.Systems.Games.CommandEvironment
{
	internal class ValidStartAtMembersString : AbstractArgumentValidator<string>
	{
		protected override ValidationFailResult? Validate(UserCommandContext context, UserCommandArgument argument, string value)
		{
			if (value.StartsWith("req#")) return int.TryParse(value[4..], out _) ? null : new("FormatError", UserCommandCode.InvalidInputFormat);
			else return int.TryParse(value, out _) ? null : new("FormatError", UserCommandCode.InvalidInputFormat);
		}
	}
}
