using FluentValidation;

namespace CGZBot3.UserCommands.Validators
{
	public class CommandArgumentValidator : AbstractValidator<UserCommandInfo.Argument>
	{
		public CommandArgumentValidator()
		{
			RuleFor(x => x.Name).Matches("[a-zA-Z]+");
		}
	}
}
