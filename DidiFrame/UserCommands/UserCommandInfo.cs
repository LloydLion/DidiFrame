using DidiFrame.UserCommands.ArgumentsValidation;
using DidiFrame.UserCommands.InvokerFiltartion;

namespace DidiFrame.UserCommands
{
	public delegate Task<UserCommandResult> UserCommandHandler(UserCommandContext ctx);


	public record UserCommandInfo(string Name, UserCommandHandler Handler, IReadOnlyList<UserCommandInfo.Argument> Arguments, IStringLocalizer Localizer, IReadOnlyCollection<IUserCommandInvokerFilter> InvokerFilters)
	{
		public record Argument(bool IsArray, IReadOnlyList<Argument.Type> OriginTypes, Type TargetType, string Name, IReadOnlyCollection<IUserCommandArgumentValidator> Validators, IReadOnlyCollection<IUserCommandArgumentPreValidator> PreValidators)
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
