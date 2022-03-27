using CGZBot3.UserCommands.ArgumentsValidation;

namespace CGZBot3.UserCommands
{
	public delegate Task<UserCommandResult> UserCommandHandler(UserCommandContext ctx);


	public record UserCommandInfo(string Name, UserCommandHandler Handler, IReadOnlyList<UserCommandInfo.Argument> Arguments, IStringLocalizer Localizer)
	{
		public record Argument(bool IsArray, Argument.Type ArgumentType, string Name, IReadOnlyCollection<IUserCommandArgumentValidator> Validators)
		{
			public enum Type
			{
				Integer,
				Double,
				String,
				Member,
				Role,
				Mentionable,
				TimeSpan
			}
		}
	}
}
