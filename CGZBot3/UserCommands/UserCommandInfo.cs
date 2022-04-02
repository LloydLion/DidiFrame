using CGZBot3.UserCommands.ArgumentsValidation;
using CGZBot3.UserCommands.InvokerFiltartion;

namespace CGZBot3.UserCommands
{
	public delegate Task<UserCommandResult> UserCommandHandler(UserCommandContext ctx);


	public record UserCommandInfo(string Name, UserCommandHandler Handler, IReadOnlyList<UserCommandInfo.Argument> Arguments, IStringLocalizer Localizer, IReadOnlyCollection<IUserCommandInvokerFilter> InvokerFilters)
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
				TimeSpan,
				DateTime
			}
		}
	}
}
