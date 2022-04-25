using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ArgumentsValidation;

namespace TestBot.Systems.Games.CommandEvironment
{
	internal class ValidStartAtMembersString : AbstractArgumentValidator<string>
	{
		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, string value)
		{
			if (value.StartsWith("req#")) return int.TryParse(value[4..], out _) ? null : "FormatError";
			else return int.TryParse(value, out _) ? null : "FormatError";
		}
	}
}
