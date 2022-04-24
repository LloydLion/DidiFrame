using FluentValidation;

namespace DidiFrame.UserCommands.Validators
{
	public class CommandArgumentValidator : AbstractValidator<UserCommandInfo.Argument>
	{
		public CommandArgumentValidator()
		{
			RuleFor(x => x.Name).Matches("[a-zA-Z]+");
		}
	}
}
