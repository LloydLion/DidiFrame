using DidiFrame.UserCommands.ArgumentsValidation;

namespace DidiFrame.UserCommands.Models
{
	public record UserCommandArgument(bool IsArray, IReadOnlyList<UserCommandArgument.Type> OriginTypes, Type TargetType, string Name, IReadOnlyCollection<IUserCommandArgumentValidator> Validators)
	{
		public enum Type
		{
			Integer,
			Double,
			String,
			Member,
			Role,
			Mentionable,
			TimeSpan,
			DateTime
		}
	}
}
