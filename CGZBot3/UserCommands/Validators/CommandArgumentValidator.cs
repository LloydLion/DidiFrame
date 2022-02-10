using FluentValidation;

namespace CGZBot3.UserCommands.Validators
{
	internal class CommandArgumentValidator : AbstractValidator<UserCommandInfo.Argument>
	{
		public CommandArgumentValidator()
		{
			RuleFor(x => x.Name).Matches("[a-zA-Z]+");
		}
	}
}
