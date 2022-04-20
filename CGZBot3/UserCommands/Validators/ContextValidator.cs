using FluentValidation;

namespace CGZBot3.UserCommands.Validators
{
	public class ContextValidator : AbstractValidator<UserCommandContext>
	{
		public ContextValidator(IValidator<UserCommandInfo> cmdInfoVal)
		{
			RuleFor(s => s.Command).SetValidator(cmdInfoVal);
			RuleFor(s => s.Invoker.Server).Equal(s => s.Channel.Server);
		}
	}
}
